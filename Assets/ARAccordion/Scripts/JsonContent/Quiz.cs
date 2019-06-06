using System.Collections.Generic;

namespace jsonObject
{
	public class Quiz {
		public IList<Questions> questions { get; set; }
		public string resultText { get; set; }
	}
}