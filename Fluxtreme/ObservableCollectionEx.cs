using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fluxtreme
{
    public class ObservableCollectionEx<T> : ObservableCollection<T>
    {
        private bool notifySupressed = false;
        private bool notifyOnResume = false;

        public ObservableCollectionEx()
        {
        }

        public ObservableCollectionEx(IEnumerable<T> collection) : base(collection)
        {
        }

        /// <summary>
        /// Pauses the CollectionChanged notifications while changes are made to this collection.
        /// Call ResumeNotifications after making changes to this collection to restore normal behavior.
        /// </summary>
        public void SupressNotifications()
        {
            notifySupressed = true;
        }

        /// <summary>
        /// Resumes notifications through the CollectionChanged event when changes are made to this collection.
        /// If any changed were made while notifications were paused, this will raise CollectionChanged with NotifyCollectionChangedAction.Reset parameter.
        /// </summary>
        public void ResumeNotifications()
        {
            notifySupressed = false;
            if (notifyOnResume)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                notifyOnResume = false;
            }
        }

        /// <inheritdoc/>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (notifySupressed)
            {
                notifyOnResume = true;
            }
            else
            {
                base.OnCollectionChanged(e);
            }
        }
    }
}
