using System.Collections.Generic;

namespace Model
{
    public class Question
    {
        public string question { get; set; }
        public List<string> answers { get; set; }
        public int correctAnswerIndex { get; set; }
    }
}