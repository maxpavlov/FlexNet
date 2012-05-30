using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Eclipse.IndexingService;
using Ionic.Zip;
using iTextSharp.text.pdf;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.Diagnostics;
using System.Xml;
using System.Diagnostics;

namespace SenseNet.Search
{
    public interface ITextExtractor
    {
        /// <summary>
        /// Extracts all relevant text information from the passed stream. Do not catch any exception but throw if it is needed.
        /// </summary>
        /// <param name="stream">Input stream</param>
        /// <returns>Extracted text</returns>
        string Extract(Stream stream);
    }

    public abstract class TextExtractor : ITextExtractor
    {
        public abstract string Extract(Stream stream);

        public static string GetExtract(BinaryData binaryData, Node node)
        {
            if (binaryData == null)
                return string.Empty;
            var fname = binaryData.FileName;
            if (fname == null)
                return string.Empty;
            var ext = fname.Extension;
            if (String.IsNullOrEmpty(ext))
                return string.Empty;

            ITextExtractor extractor = null;
            var result = string.Empty;
            switch (ext.ToLower())
            {
                case "contenttype":
                case "xml": extractor = new XmlTextExtractor(); break;
                case "doc": extractor = new DocTextExtractor(); break;
                case "xls": extractor = new XlsTextExtractor(); break;
                case "pdf": extractor = new PdfTextExtractor(); break;
                case "docx": extractor = new DocxTextExtractor(); break;
                case "xlsx": extractor = new XlsxTextExtractor(); break;
                case "pptx": extractor = new PptxTextExtractor(); break;
                case "txt": extractor = new PlainTextExtractor(); break;
                default:
                    return String.Empty;
            }

            var stream = binaryData.GetStream();
            if (stream == null)
                return String.Empty;
            if (stream.Length == 0)
                return String.Empty;

            try
            {
                ////-- sync
                //result = extractor.Extract(stream);

                //-- async
                Action<TimeboxedActivity> timeboxedFunctionCall = activity =>
                {
                    var x = (Stream)activity.InArgument;
                    var extract = extractor.Extract(x);
                    activity.OutArgument = extract;
                };

                var act = new TimeboxedActivity();
                act.InArgument = stream;
                act.Activity = timeboxedFunctionCall;

                var finishedWithinTime = act.ExecuteAndWait(5000);
                if (!finishedWithinTime)
                {
                    act.Abort();
                    var msg = String.Format("Text extracting timeout. Version: {0}, path: {1}", node.Version, node.Path);
                    Logger.WriteWarning(msg);
                    return String.Empty;
                }
                else if (act.ExecutionException != null)
                {
                    WriteError(act.ExecutionException, node);
                }
                else
                {
                    result = (string)act.OutArgument;
                }
            }
            catch (Exception e)
            {
                WriteError(e, node);
            }

            if (String.IsNullOrEmpty(result))
            {
                var format = @"Couldn't extract text. VersionId: {0}, path: '{1}' ";
                var inf = String.Format(CultureInfo.InvariantCulture, format, node.VersionId, node.Path);
                Logger.WriteWarning(inf);
            }

            result = result.Replace('\0', '.');
            return result;
        }
        public static string GetExtract(Stream stream, string fileName, out string errorMessage)
        {
            if (stream == null)
            {
                errorMessage = null;
                return String.Empty;
            }
            if (stream.Length == 0)
            {
                errorMessage = null;
                return String.Empty;
            }
            if (String.IsNullOrEmpty(fileName))
            {
                errorMessage = "Cannot resolve a TextExtractor if FileName is null or empty";
                return String.Empty;
            }
            var extension = Path.GetExtension(fileName);
            if (String.IsNullOrEmpty(extension))
            {
                errorMessage = "Cannot resolve a TextExtractor if FileName's extension is null or empty";
                return string.Empty;
            }
            extension = extension.TrimStart('.');
            if (extension.Length == 0)
            {
                errorMessage = "Cannot resolve a TextExtractor if FileName's extension is empty";
                return string.Empty;
            }
            extension = extension.ToLower();
            if (extension == "txt")
            {
                errorMessage = null;
                return SenseNet.ContentRepository.Tools.GetStreamString(stream);
            }

            ITextExtractor extractor = null;
            var result = string.Empty;
            switch (extension)
            {
                case "contenttype":
                case "xml": extractor = new XmlTextExtractor(); break;
                case "doc": extractor = new DocTextExtractor(); break;
                case "xls": extractor = new XlsTextExtractor(); break;
                case "pdf": extractor = new PdfTextExtractor(); break;
                case "docx": extractor = new DocxTextExtractor(); break;
                case "xlsx": extractor = new XlsxTextExtractor(); break;
                case "pptx": extractor = new PptxTextExtractor(); break;
                case "txt": extractor = new PlainTextExtractor(); break;
                default:
                    errorMessage = String.Format("Cannot resolve a TextExtractor for this extension: '{0}'", extension);
                    return String.Empty;
            }

            try
            {
                //-- sync
                result = extractor.Extract(stream);
                errorMessage = null;

                ////-- async
                /*
                Action<TimeboxedActivity> timeboxedFunctionCall = activity =>
                {
                    var x = (Stream)activity.InArgument;
                    var extract = extractor.Extract(x);
                    activity.OutArgument = extract;
                };

                var act = new TimeboxedActivity();
                act.InArgument = stream;
                act.Activity = timeboxedFunctionCall;

                var finishedWithinTime = act.ExecuteAndWait(5000);
                if (!finishedWithinTime)
                {
                    act.Abort();
                    errorMessage = String.Format("Text extracting timeout. path: {0}", fileName);
                    return String.Empty;
                }
                else if (act.ExecutionException != null)
                {
                    errorMessage = String.Format("An error occured during extracting text. Path: {0}. Message: {1}", fileName, act.ExecutionException.Message);
                }
                else
                {
                    result = (string)act.OutArgument;
                    errorMessage = null;
                }
                */
            }
            catch (Exception e)
            {
                errorMessage = String.Format("An error occured during extracting text. Path: {0}. Message: {1}", fileName, e.Message);
            }

            if (String.IsNullOrEmpty(result))
            {
                var format = @"Couldn't extract text. FileName: '{0}' ";
                errorMessage = String.Format(CultureInfo.InvariantCulture, format, fileName);
            }

            result = result.Replace('\0', '.');
            return result;
        }

        private static void WriteError(Exception e, Node node)
        {
            Logger.WriteError(String.Format("An error occured during extracting text.  Version: {0}, path: {1}", node.Version, node.Path), Logger.GetDefaultProperties(e));
        }
        protected string GetOpenXmlText(Stream stream)
        {
            //Solution #1: ORIGINAL, openxml reader
            //var sw = new Stopwatch();
            //sw.Start();

            var result = new StringBuilder();
            using (var zip = ZipFile.Read(stream))
            {
                foreach (var entry in zip)
                {
                    if (Path.GetExtension(entry.FileName.ToLower()).Trim('.') == "xml")
                    {
                        var zipStream = new MemoryStream();
                        entry.Extract(zipStream);
                        zipStream.Seek(0, SeekOrigin.Begin);
                        var extractedText = new XmlTextExtractor().Extract(zipStream);
                        if (String.IsNullOrEmpty(extractedText))
                        {
                            zipStream.Close();
                            continue;
                        }
                        result.Append(extractedText);
                        zipStream.Close();
                    }
                }
            }
            
            //sw.Stop();
            //WriteElapsedLog(sw, "openxml reader", stream.Length);

            ////reset the stream
            //stream.Seek(0, SeekOrigin.Begin);
            //result = new StringBuilder();

            //sw.Restart();

            ////Solution #2: IFilter
            //var target = new FilterReader(GetBytesFromStream(stream));
            //target.Init();
            //result.Append(target.ReadToEnd());

            //sw.Stop();
            //WriteElapsedLog(sw, "IFilter      ", stream.Length);

            return result.ToString();
        }

        protected static void WriteElapsedLog(Stopwatch sw, string message, long length)
        {
            Trace.WriteLine( string.Format(">>>>>>> Text extract **** {0} **** {1} ms **** length: {2}", message ?? string.Empty, sw.ElapsedMilliseconds.ToString().PadLeft(5), length));
        }

        protected static byte[] GetBytesFromStream(Stream stream)
        {
            byte[] fileData;
            if (stream is MemoryStream)
            {
                fileData = ((MemoryStream)stream).ToArray();
            }
            else
            {
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    fileData = ms.ToArray();
                }
            }

            return fileData;
        }
    }

    internal sealed class DocxTextExtractor : TextExtractor
    {
        public override string Extract(Stream stream)
        {
            return base.GetOpenXmlText(stream);
        }
    }
    internal sealed class XlsxTextExtractor : TextExtractor
    {
        public override string Extract(Stream stream)
        {
            return base.GetOpenXmlText(stream);
        }
    }
    internal sealed class PptxTextExtractor : TextExtractor
    {
        public override string Extract(Stream stream)
        {
            return base.GetOpenXmlText(stream);
        }
    }
    internal sealed class PdfTextExtractor : TextExtractor
    {
        public override string Extract(Stream stream)
        {
            var extractedTexts = new StringBuilder();
            var pdfReader = new PdfReader(stream);
            for (var page = 1; page <= pdfReader.NumberOfPages; page++)
            {
                var text = ExtractTextFromPdfBytes(pdfReader.GetPageContent(page));
                if (String.IsNullOrEmpty(text))
                    continue;
                extractedTexts.Append(text);
            }
            return extractedTexts.ToString();
        }
        private static string ExtractTextFromPdfBytes(byte[] input)
        {
            if (input == null || input.Length == 0)
                return "";
            var result = new StringBuilder();
            var token = new PRTokeniser(input);
            while (token.NextToken())
            {
                var tknType = token.TokenType;
                var tknValue = token.StringValue;

                if (tknType == PRTokeniser.TokType.STRING)
                    result.Append(token.StringValue);
                else switch (tknValue)
                {
                    case "-600":
                        result.Append(" ");
                        break;
                    case "TJ":
                        result.Append(" ");
                        break;
                }
            }
            return result.ToString();
        }
    }
    internal sealed class XmlTextExtractor : TextExtractor
    {
        public override string Extract(Stream stream)
        {
            // initial length: chars = bytes / 2, relevant text rate: ~25%
            var sb = new StringBuilder(Math.Max(20, Convert.ToInt32(stream.Length / 8)));
            var reader = new XmlTextReader(stream);
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Text && reader.HasValue)
                {
                    sb.Append(reader.Value).Append(' ');
                }
            }

            return sb.ToString();
        }
    }
    internal sealed class DocTextExtractor : TextExtractor
    {
        public override string Extract(System.IO.Stream stream)
        {
            //IFilter
            var target = new FilterReader(GetBytesFromStream(stream), ".doc");
            target.Init();
            return target.ReadToEnd();
        }
    }
    internal sealed class XlsTextExtractor : TextExtractor
    {
        public override string Extract(Stream stream)
        {
            //IFilter
            var target = new FilterReader(GetBytesFromStream(stream), ".xls");
            target.Init();
            return target.ReadToEnd();
        }
    }

    internal sealed class PlainTextExtractor : TextExtractor
    {
        public override string Extract(Stream stream)
        {
            return Tools.GetStreamString(stream);
        }
    }
}
