﻿using Networks.Data;
using QuickGraph;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Media;

namespace Networks
{
    /// <summary>
    /// A simple identifiable edge.
    /// </summary>
    /// 
    [DebuggerDisplay("{Source.ID} -> {Target.ID}")]
    [Serializable]
    public class PocEdge : TaggedEdge<PocVertex, object>, INotifyPropertyChanged
    {
        private string id;
        public Color EdgeColor { get; set; }
        public Int32 weight { get; set; }
        public Int32 Bandwidth { get; set; }
        public LinkType linkType { get; set; }

        public Double ErrorProbability { get; set; }
        public string ID
        {
            get { return id; }
            set
            {
                id = value;
                NotifyPropertyChanged("ID");
            }
        }

        public PocEdge(string id, PocVertex source, PocVertex target, object tag)
            : base(source, target, tag)
        {
            ID = id;
            linkType = LinkType.Normal;
            Bandwidth = 1024;
            ErrorProbability = new Random().Next(1, 6);
            EdgeColor = Colors.YellowGreen;
            ToolTip = weight.ToString();
        }
        public override string ToString()
        {
            return ToolTip;
        }

        private string _toolTip;
        public string ToolTip
        {
            get { return _toolTip; }
            set { _toolTip = value; }
        }

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }       
        #endregion
    }
}
