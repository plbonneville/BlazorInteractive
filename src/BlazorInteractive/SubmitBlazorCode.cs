using System.Text.RegularExpressions;
using Microsoft.DotNet.Interactive.Commands;

namespace BlazorInteractive
{
    public class SubmitBlazorCode : SubmitCode
    {
        public const string DefaultComponentName = "__Main";

        public SubmitBlazorCode(string componentName, string code, string targetKernelName = null, SubmissionType submissionType = SubmissionType.Run)
            : base(code, targetKernelName, submissionType)
        {
            if (string.IsNullOrWhiteSpace(componentName))
            {
                componentName = DefaultComponentName;
            }

            ComponentName = componentName;
        }

        public string ComponentName { get; }

        public static SubmitBlazorCode From(SubmitCode submitCode, string componentName = DefaultComponentName)
        {
            var regex = new Regex("^#!blazor");
            var code = regex.Replace(submitCode.Code, "#!blazor-kernel");

            var command = new SubmitBlazorCode(componentName, code, submitCode.TargetKernelName, submitCode.SubmissionType);

            return command;
        }
    }
}