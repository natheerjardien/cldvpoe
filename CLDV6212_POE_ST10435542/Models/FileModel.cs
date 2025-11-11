namespace CLDV6212_POE_ST10435542.Models
{
    public class FileModel
    {
    // As demonstarted by IIEVC School of Computer Science (2025), the FileModel class represents a file in the Azure File Share
    // Ive followed the same structure as demonstrated, with properties for the file name, size, and last modified date
        public string Name { get; set; } // name of the file
        public long Size { get; set; } // size of the file in bytes
        public DateTimeOffset? LastModified { get; set; }

        public string DisplaySize // displays the size of the file
        {
            get
            {
                if (Size >= 1024 * 1024)
                {
                    return $"{Size / 1024 / 1024} MB";
                }
                if (Size >= 1024)
                {
                    return $"{Size / 1024} KB";
                }

                return $"{Size} Bytes";
            }
        }
    }
}

/* References:

IIEVC School of Computer Science, 2025. CLDV6212 ASP.NET MVC & Azure Series - Part 4: Mastering Azure File Share!. [video online] 
Available at: <https://youtu.be/A-mVVL88oEg?si=eIL4gyih_S6aWw2a>
[Accessed 21 August 2025].

*/
