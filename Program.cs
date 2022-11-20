using EmailRegex;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

internal class Program
{
    static List<string> emails = new List<string>();

    static int SUCCESS_TESTS = 0;
    static int FAILED_TESTS = 0;
    static int RUNNED_TESTS = 0;

    private static void Main(string[] args)
    {
        ReadEmails();
        RunTests(0, 37);
        Console.WriteLine($"Tests count({RUNNED_TESTS}): Success: {SUCCESS_TESTS} Failures: {FAILED_TESTS}");
    }

    static void ReadEmails()
    {
        var rows = File.ReadAllLines("D:\\Iskola\\VargaGábor\\EmailRegex\\emails.txt");
        foreach (var row in rows)
        {
            emails.Add(row);
        }
    }

    class RegexUtilities
    {
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    string domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException e)
            {
                return false;
            }
            catch (ArgumentException e)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
    }

    static void RunTests(int start, int end)
    {
        List<string> succcessEmails = new List<string>();
        List<string> syntaxFailedEmails = new List<string>();
        List<string> domainErrorEmails = new List<string>();

        for (int i = start; i < end; i++)
        {
            string email = emails[i];

            RUNNED_TESTS++;
            if (RegexUtilities.IsValidEmail(email))
            {
                if (_IsValidDomain(email))
                {
                    Console.WriteLine($"Valid: {email}");
                    SUCCESS_TESTS++;
                    succcessEmails.Add(email);
                }
                else
                {
                    Console.WriteLine($"Invalid (Domain Error): {email}");
                    FAILED_TESTS++;
                    domainErrorEmails.Add(email);
                }
            }
            else
            {
                Console.WriteLine($"Invalid (RegEx Syntax): {email}");
                FAILED_TESTS++;
                syntaxFailedEmails.Add(email);
            }
        }

        File.WriteAllLines("D:\\Iskola\\VargaGábor\\EmailRegex\\syntaxFailedEmails.txt", syntaxFailedEmails);
        File.WriteAllLines("D:\\Iskola\\VargaGábor\\EmailRegex\\domainErrorEmails.txt", domainErrorEmails);
        File.WriteAllLines("D:\\Iskola\\VargaGábor\\EmailRegex\\successEmails.txt", succcessEmails);
    }

    static bool _IsValidDomain(string email)
    {
        string domain = email.Split("@")[1];
        try
        {
            IPHostEntry entry = Dns.GetHostEntry(domain);
            if (entry != null)
            {
                return true;
            }
        }
        catch
        { }
        return false;
    }
}