using HandlebarsDotNet;
using System;
using System.IO;

namespace RocketChatToDoServer
{
    public static class HandlebarsSetup
    {
        public static void Setup()
        {
            void notDefaultDateHelper(TextWriter output, HelperOptions options, dynamic context, object[] arguments)
            {
                if (arguments.Length != 1)
                {
                    throw new HandlebarsException("{{NotDefaultDateHelper}} helper must have exactly one argument");
                }
                var arg = (DateTime)arguments[0];
                if (arg != default)
                {
                    options.Template(output, context);
                }
                else
                {
                    options.Inverse(output, context);
                }
            }
            Handlebars.RegisterHelper("NotDefaultDate", notDefaultDateHelper);

            Handlebars.RegisterHelper("FormatDate", (writer, context, parameters) =>
            {
                var date = (DateTime)parameters[0];
                string format = parameters[1] as string;
                writer.WriteSafeString(date.ToString(format));
            });
        }
    }
}
