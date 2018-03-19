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
        private List<string> equivalenceLabels;
        private List<int> indexes;

        public LabelingHandler(List<string> data)
        {
            labels = data;

            equivalenceLabels = new List<string>();
            indexes = new List<int>();

            for(int i = 0; i < labels.Count; i++)
            {
                if (!equivalenceLabels.Contains(labels[i]))
                {
                    equivalenceLabels.Add(labels[i]);
                    indexes.Add(equivalenceLabels.Count);
                }
                else
                {
                    indexes.Add(equivalenceLabels.IndexOf(labels[i]) + 1);
                }
            }
        }

        public int getIndex(string label)
        {
            return (equivalenceLabels.IndexOf(label) + 1);
        }

        public int getIndex(int position)
        {
            return indexes[position];
        }

        public string getLabel(int index)
        {
            return equivalenceLabels[index];
        }

        public List<int> getAllIndexes()
        {
            return indexes;
        }
    }
}
