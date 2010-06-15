using System.ComponentModel.Composition;
using System.Text.RegularExpressions;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace CancelFailedBuild
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("BuildOutput")]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    sealed class OutputWindowCreationListener : IWpfTextViewCreationListener
    {
        [Import]
        SVsServiceProvider GlobalServiceProvider = null;

        static readonly Regex BuildError = new Regex(": error", RegexOptions.IgnoreCase);

        public void TextViewCreated(IWpfTextView textView)
        {
            var dte = GlobalServiceProvider.GetService(typeof(DTE)) as DTE;
            textView.TextBuffer.Changed += (sender, args) =>
                {
                    //Output window is friendly and writes full lines at a time, so we only need to look at the changed text.
                    foreach (var change in args.Changes)
                    {
                        string text = args.After.GetText(change.NewSpan);
                        if (BuildError.IsMatch(text))
                        {
                            dte.ExecuteCommand("Build.Cancel");
                        }
                    }
                };
        }
    }
}
