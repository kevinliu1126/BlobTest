using System;

namespace BlobTest.Models
{
    public class GetFileModel
    {
        public int File_ID { get; set; }
        public string Filename { get; set; }
        public DateTime Input_time { get; set; }
        public string Save_name { get; set; }
    }
}
