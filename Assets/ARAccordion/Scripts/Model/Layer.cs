using System.Collections.Generic;
using UnityEngine;

namespace Model
{
    public class Layer
    {
        public string id;
        public List<Info> infos;
        public List<Question> questions;
        public int[] color;
        public string type;
    }
}