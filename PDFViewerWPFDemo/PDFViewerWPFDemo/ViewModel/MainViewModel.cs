using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using pdftron;
using pdftron.PDF;
using pdftron.PDF.Tools;
using pdftron.SDF;

namespace PDFViewerWPFDemo.ViewModel
{
    class MainViewModel : BaseViewModel
    {
        TextSearch textSearch;

        ToolManager _toolManager;
        UndoManager _undoManager;

        public MainViewModel()
        {
            // Initilizes PDFNet
            PDFNet.Initialize("demo:1629991648523:78e4805d03000000005e485462887551161519118686561bf312807cdd");

            textSearch = new TextSearch();

            // Make sure to Terminate any processes
            Application.Current.SessionEnding += Current_SessionEnding;

            // Init all Commands
            CMDOpenDocument = new Relaycommand(OpenDocument);
            CMDNextPage = new Relaycommand(NextPage);
            CMDPreviousPage = new Relaycommand(PreviousPage);
                        
            // Annotations
            CMDAnottateText = new Relaycommand(AddTextSample);
            CMDFreeTextCreate = new Relaycommand(AddFreeTextSample);
            CMDSelectText = new Relaycommand(SelectText);
            CMDSquareCreate = new Relaycommand(AddSquareAnnotation);
            CMDArrowCreate = new Relaycommand(AddArrowAnnotation);
            CMDOvalCreate = new Relaycommand(AddOvalAnnotation);
            CMDSquigglyCreate = new Relaycommand(AddSquigglyAnnotation);
            CMDUnderlineCreate = new Relaycommand(AddUnderlineAnnotation);
            CMDStrikeoutCreate = new Relaycommand(AddStrikeoutAnnotation);
            CMDTextHighlightCreate = new Relaycommand(AddHighlightAnnotation);
            CMDInkCreate = new Relaycommand(AddInkAnnotation);
            CMDStampImage = new Relaycommand(StampImage);

            CMDExit = new Relaycommand(ExitApp);
            CMDZoomIn = new Relaycommand(ZoomIn);
            CMDZoomOut = new Relaycommand(ZoomOut);
            CMDUndo = new Relaycommand(Undo);
            CMDRedo = new Relaycommand(Redo);
                       
            // Checks the scale factor to determine the right resolution
            PresentationSource source = PresentationSource.FromVisual(Application.Current.MainWindow);
            double scaleFactor = 1;
            if (source != null)
            {
                scaleFactor = 1 / source.CompositionTarget.TransformFromDevice.M11;
            }
            
            // Set working doc to Viewer
            PDFViewer = new PDFViewWPF();
            PDFViewer.PixelsPerUnitWidth = scaleFactor;
            PDFViewer.SetPagePresentationMode(PDFViewWPF.PagePresentationMode.e_single_continuous);
            PDFViewer.AllowDrop = true;

            // PDF Viewer Events subscription
            PDFViewer.MouseLeftButtonDown += PDFView_MouseLeftButtonDown;
            PDFViewer.Drop += PDFViewer_Drop;

            // Enable access to the Tools available
            _toolManager = new ToolManager(PDFViewer);
            _toolManager.AnnotationAdded += _toolManager_AnnotationAdded;
            _toolManager.AnnotationRemoved += _toolManager_AnnotationRemoved;

            // Load PDF file
            PDFDoc doc = new PDFDoc("./Resources/GettingStarted.pdf");
            doc.InitSecurityHandler();
            _undoManager = doc.GetUndoManager();
            PDFViewer.SetDoc(doc);
        }

        #region Public Properties

        PDFViewWPF _pDFView;
        public PDFViewWPF PDFViewer 
        { 
            get { return _pDFView; } 
            set 
            {
                _pDFView = value; 
                NotifyPropertyChanged();
            } 
        }

        string _windowTitle = "PDFTron WPF Sample App";
        public string WindowTitle { get { return _windowTitle; } set { _windowTitle = value; } }

        public bool ToolsEnabled { get { return PDFViewer.CurrentDocument == null ? false : true; } }

        #endregion

        #region Commands

        public ICommand CMDOpenDocument { get; set; }

        public ICommand CMDNextPage { get; set; }

        public ICommand CMDPreviousPage { get; set; }

        public ICommand CMDAnottateText { get; set; }

        public ICommand CMDFreeTextCreate { get; set; }

        public ICommand CMDSquareCreate { get; set; }

        public ICommand CMDArrowCreate { get; set; }

        public ICommand CMDOvalCreate { get; set; }

        public ICommand CMDSquigglyCreate { get; set; }

        public ICommand CMDUnderlineCreate { get; set; }

        public ICommand CMDStrikeoutCreate { get; set; }

        public ICommand CMDTextHighlightCreate { get; set; }

        public ICommand CMDInkCreate { get; set; }

        public ICommand CMDExit { get; set; }

        public ICommand CMDZoomIn { get; set; }

        public ICommand CMDZoomOut { get; set; }

        public ICommand CMDSelectText { get; set; }

        public ICommand CMDUndo { get; set; }

        public ICommand CMDRedo { get; set; }

        public ICommand CMDStampImage { get; set; }
        #endregion

        #region Operations

        // NOTE: Zoom will perform increments of 10%
        private void ZoomIn() 
        { 
            PDFViewer.Zoom +=  PDFViewer.GetZoom() * 0.1;
        }
        private void ZoomOut() 
        {
            PDFViewer.Zoom -= PDFViewer.GetZoom() * 0.1;
        }

        private void Undo()
        {
            if (_undoManager == null)
                return;

            if (!_undoManager.CanUndo())
                return;

            _undoManager.Undo();

            PDFViewer.Update(); // PDFViewer updates display
        }

        private void Redo()
        {
            if (_undoManager == null)
                return;

            if (!_undoManager.CanRedo())
                return;

            _undoManager.Redo();

            PDFViewer.Update(); // PDFViewer updates display
        }

        /// <summary>
        /// It open dialog to load a PDF File
        /// </summary>
        private void OpenDocument()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var doc = new PDFDoc(openFileDialog.FileName);
                    doc.InitSecurityHandler();

                    PDFViewer.CurrentDocument = doc;
                    _undoManager = doc.GetUndoManager(); // Get document Undo Redo Manager

                    NotifyPropertyChanged(nameof(ToolsEnabled));
                }
                catch (Exception)
                {
                    throw new Exception("OpenDocument() failed");
                }
            }            
        }

        private void NextPage() { PDFViewer.GotoNextPage(); }

        private void PreviousPage() { PDFViewer.GotoPreviousPage(); }

        private void AddTextSample()
        {
            var workingDoc = PDFViewer.CurrentDocument;
            Page page = workingDoc.GetPage(PDFViewer.CurrentPageNumber);

            if (page == null) return;

            _toolManager.CreateTool(ToolManager.ToolType.e_sticky_note_create);

            PDFViewer.Update();
        }

        private void AddFreeTextSample()
        {
            var workingDoc = PDFViewer.CurrentDocument;
            Page page = workingDoc.GetPage(PDFViewer.CurrentPageNumber);

            if (page == null) return;

            _toolManager.CreateTool(ToolManager.ToolType.e_text_annot_create);
        }

        private void SelectText()
        {
            _toolManager.CreateTool(ToolManager.ToolType.e_annot_edit);
        }

        private void AddSquareAnnotation()
        {
            var workingDoc = PDFViewer.CurrentDocument;
            Page page = workingDoc.GetPage(PDFViewer.CurrentPageNumber);

            if (page == null) return;

            _toolManager.CreateTool(ToolManager.ToolType.e_rect_create);
        }

        private void AddArrowAnnotation()
        {
            var workingDoc = PDFViewer.CurrentDocument;
            Page page = workingDoc.GetPage(PDFViewer.CurrentPageNumber);

            if (page == null) return;

            _toolManager.CreateTool(ToolManager.ToolType.e_arrow_create);
        }

        private void AddOvalAnnotation()
        {
            var workingDoc = PDFViewer.CurrentDocument;
            Page page = workingDoc.GetPage(PDFViewer.CurrentPageNumber);

            if (page == null) return;

            _toolManager.CreateTool(ToolManager.ToolType.e_oval_create);
        }

        private void AddSquigglyAnnotation()
        {
            var workingDoc = PDFViewer.CurrentDocument;
            Page page = workingDoc.GetPage(PDFViewer.CurrentPageNumber);

            if (page == null) return;

            _toolManager.CreateTool(ToolManager.ToolType.e_text_squiggly);
        }

        private void AddUnderlineAnnotation()
        {
            var workingDoc = PDFViewer.CurrentDocument;
            Page page = workingDoc.GetPage(PDFViewer.CurrentPageNumber);

            if (page == null) return;

            _toolManager.CreateTool(ToolManager.ToolType.e_text_underline);
        }

        private void AddStrikeoutAnnotation()
        {
            var workingDoc = PDFViewer.CurrentDocument;
            Page page = workingDoc.GetPage(PDFViewer.CurrentPageNumber);

            if (page == null) return;

            _toolManager.CreateTool(ToolManager.ToolType.e_text_strikeout);
        }

        private void AddHighlightAnnotation()
        {
            var workingDoc = PDFViewer.CurrentDocument;
            Page page = workingDoc.GetPage(PDFViewer.CurrentPageNumber);

            if (page == null) return;

            _toolManager.CreateTool(ToolManager.ToolType.e_text_highlight);
        }

        private void AddInkAnnotation()
        {
            var workingDoc = PDFViewer.CurrentDocument;
            Page page = workingDoc.GetPage(PDFViewer.CurrentPageNumber);

            if (page == null) return;

            _toolManager.CreateTool(ToolManager.ToolType.e_ink_create);
        }

        private void StampImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image (*.jpg) | *.jpg";
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    PDFViewer.DocLock(true);

                    // Embed a JPEG image to the output document. 
                    Image img = Image.Create(PDFViewer.GetDoc().GetSDFDoc(), openFileDialog.FileName);

                    Stamper s = new Stamper(Stamper.SizeType.e_relative_scale, .05, .05);
                    s.SetSize(Stamper.SizeType.e_relative_scale, 0.5, 0.5);
                    //set position of the image to the center, left of PDF pages
                    s.SetAlignment(Stamper.HorizontalAlignment.e_horizontal_left, Stamper.VerticalAlignment.e_vertical_center);
                    s.SetFontColor(new ColorPt(0, 0, 0, 0));
                    s.SetRotation(180);
                    s.SetAsBackground(false);

                    //only stamp on the selected page
                    int curPage = PDFViewer.CurrentPageNumber;
                    s.StampImage(PDFViewer.GetDoc(), img, new PageSet(curPage, curPage));

                    PDFViewer.Update(); // refresh viewer
                }
                catch (Exception)
                {
                    throw new Exception("Image Stamp failed");
                }
                finally
                {
                    PDFViewer.DocUnlock();
                }
            }
        }

        private void ExitApp() 
        {
            Application.Current.Shutdown();
        }
        
        #endregion

        #region Events
        private void Current_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            PDFNet.Terminate();
        }

        private void _toolManager_AnnotationRemoved(Annot annotation)
        {
            TakeSnapshot();
        }

        private void _toolManager_AnnotationAdded(Annot annotation)
        {
            TakeSnapshot();

            // Update viewer after annotation added
            PDFViewer.Update(annotation, PDFViewer.GetCurrentPage());
        }

        private void TakeSnapshot()
        {
            bool isLocked = false;
            try
            {
                PDFViewer.DocLock(true);
                isLocked = true;

                ResultSnapshot snap = _undoManager.TakeSnapshot();

            }
            catch { }
            finally
            {
                if (isLocked)
                    PDFViewer.DocUnlock();
            }
        }

        /// <summary>
        /// On Left Mouse Button Down checks which tool is selected and anottate
        /// </summary>
        private void PDFView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var workingDoc = PDFViewer.CurrentDocument;
            if (workingDoc == null)
                return;

            var clickPos = e.GetPosition(PDFViewer);

            Page page = workingDoc.GetPage(1);

            if (page == null) return;
        }

        /// <summary>
        /// Handle when a file is dragged and dropped onto the PDFView Control
        /// </summary>
        private void PDFViewer_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note: Assuming only 1 file was dropped
                string[] file_paths = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (file_paths.Length > 0)
                {
                    // Let's only handle the first file dropped
                    string file_path = file_paths[0];

                    if (System.IO.Path.GetExtension(file_path) != ".pdf")
                        return;

                    PDFDoc doc = new PDFDoc(file_path);
                    PDFViewer.SetDoc(doc);
                }
            }
        }

        #endregion
    }
}
