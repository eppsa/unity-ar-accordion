using System.Collections.Generic;

namespace Model
{
    public class Layer
    {
        public int id { get; set; }
        public string information { get; set; }
        public List<Question> questions { get; set; }
    }
}