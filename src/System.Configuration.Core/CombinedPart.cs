﻿using System.Configuration.Core.Metadata;
using System.Diagnostics;

namespace System.Configuration.Core {

    internal sealed class CombinedPart : ConfigurationObjectPart {

        #region Node
        internal sealed class Node {
            public Node(ConfigurationObjectPart value,Node preNode) {
                Debug.Assert(value != null);
                this.Value = value;

                if (preNode != null) {
                    Debug.Assert(preNode.Next == null);
                    preNode.Next = this;
                }
            }

            public readonly ConfigurationObjectPart Value;
            public Node Next;
        }
        #endregion

        internal CombinedPart(Node first) {
            Debug.Assert(first != null);
            this._first = first;
        }

        private Node _first;

        protected override void OpenDataCore(OpenDataContext ctx) {
            var current = _first;
            do {
                current.Value.OpenData(ctx);//可能已经被其他workspace解包了，所以调用OpenData而不是OpenDataCore
                current = current.Next;                
            } while (current != null);
        }
        
        public override bool TryGetLocaleValue(IProperty property, out object value) {
            var current = _first;
            do {
                if (current.Value.TryGetLocaleValue(property,out value)) {
                    return true;
                }
                current = current.Next;
            } while (current != null);

            return false;
        }

        public override IType Type {
            get {
                return _first.Value.Type;
            }
        }
    }
}