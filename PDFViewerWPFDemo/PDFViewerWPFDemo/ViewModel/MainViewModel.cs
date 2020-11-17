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
        TextSearch textSearch = new TextSearch();

        ToolManager _toolManager;
        UndoManager _undoManager;

        public MainViewModel()
        {
            // Initilizes PDFNet
            PDFNet.Initialize();

            // Make sure to Terminate any processes
            Application.Current.SessionEnding += Current_SessionEnding;

            // Init all Commands
            CMDOpenDocument = new Relaycommand(OpenDocument);
            CMDNextPage = new Relaycommand(NextPage);
            CMDPreviousPage = new Relaycommand(PreviousPage);
            CMDAnottateText = new Relaycommand(AddTextSample);
            CMDFreeTextSample = new Relaycommand(AddFreeTextSample);
            CMDSelectText = new Relaycommand(SelectText);
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

            // PDF Viewer Events subscription
            PDFViewer.MouseLeftButtonDown += PDFView_MouseLeftButtonDown;
            
            // Enable access to the Tools available
            _toolManager = new ToolManager(PDFViewer);
            _toolManager.AnnotationAdded += _toolManager_AnnotationAdded;
            _toolManager.AnnotationRemoved += _toolManager_AnnotationRemoved;
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

        public 

        string _windowTitle = "PDFTron WPF Sample App";
        public string WindowTitle { get { return _windowTitle; } set { _windowTitle = value; } }

        public bool ToolsEnabled { get { return PDFViewer.CurrentDocument == null ? false : true; } }

        #endregion

        #region Commands

        public ICommand CMDOpenDocument { get; set; }

        public ICommand CMDNextPage { get; set; }

        public ICommand CMDPreviousPage { get; set; }

        public ICommand CMDAnottateText { get; set; }

        public ICommand CMDFreeTextSample { get; set; }

        public ICommand CMDExit { get; set; }

        public ICommand CMDZoomIn { get; set; }

        public ICommand CMDZoomOut { get; set; }

        public ICommand CMDSelectText { get; set; }

        public ICommand CMDUndo { get; set; }

        public ICommand CMDRedo { get; set; }
        #endregion

        #region Operations

        // TODO: adjust it to do in increments of 10%
        private void ZoomIn() { PDFViewer.Zoom += 1; }
        private void ZoomOut() { PDFViewer.Zoom -= 1; }

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

        private void CheckCanExcute(PDFViewWPF view) 
        { }

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
            ResultSnapshot snap = _undoManager.TakeSnapshot();
        }

        private void _toolManager_AnnotationAdded(Annot annotation)
        {
            ResultSnapshot snap = _undoManager.TakeSnapshot();
        }

        /// <summary>
        /// On Left Mouse Button Down checks which tool is selected and anottate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PDFView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var workingDoc = PDFViewer.CurrentDocument;
            if (workingDoc == null)
                return;

            var clickPos = e.GetPosition(PDFViewer);

            Page page = workingDoc.GetPage(1);

            if (page == null) return;
        }

        #endregion
    }
}
