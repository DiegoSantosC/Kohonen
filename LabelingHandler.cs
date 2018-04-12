using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohonen
{
    class LabelingHandler
    {
        private List<string> labels;
        private List<int> indexes;

        public LabelingHandler(List<string> data)
        {
            labels = data;

            List<string> aux = new List<string>();
            indexes = new List<int>();

            for(int i = 0; i < labels.Count; i++)
            {
                if (aux.Contains(labels[i])) indexes.Add(aux.IndexOf(labels[i]));
                else if (labels[i].Length == 0) indexes.Add(-1);
                else {indexes.Add(aux.Count); aux.Add(labels[i]); }
            }
        }

        public int getIndex(string label)
        {
            return (indexes[labels.IndexOf(label)]);
        }

        public int getIndex(int position)
        {
            return indexes[position];
        }

        public string getLabel(int index)
        {
            return labels[indexes.IndexOf(index)];
        }

        public List<int> getAllIndexes()
        {
            return indexes;
        }
        public List<string> getAllLabels()
        {
            return labels;
        }
    }
}
