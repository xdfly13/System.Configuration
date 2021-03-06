﻿using System.Collections.Generic;
using System.Configuration.Core.Collections;
using System.Configuration.Core.Metadata;

namespace System.Configuration.Core {

    internal abstract class BasicPart : ConfigurationObjectPart {
        
        private Dictionary<IProperty, object> _values;

        internal BasicPart() {
            this._values = new Dictionary<IProperty, object>(ReferenceEqualityComparer<IProperty>.Default);
        }

        #region TryGetValue

        /// <summary>
        /// 返回部件内部存储的值，如果内部有定义值将返回他。
        /// </summary>
        /// <param name="property">要检索的属性</param>
        /// <param name="value">如果检索到有效的定义将返回他，否则返回null</param>
        /// <returns>如果检索到有效的定义将返回true</returns>
        public override bool TryGetLocalValue(IProperty property, out object value) {
            return _values.TryGetValue(property, out value);
        }
        
        public override bool TryGetLocalListValue(IProperty property, out IEnumerable<ListDifferenceItem> list) {
            object value;
            if (_values.TryGetValue(property, out value)) {
                list = (IEnumerable<ListDifferenceItem>)value;
                return true;
            }

            list = null;
            return false;
        }

        protected void SetLocalValue(IProperty property, object value) {
            _values[property] = value;
        }

        protected void ResetLocalValue(IProperty property) {
            _values.Remove(property);
        }

        public override IEnumerable<KeyValuePair<IProperty, object>> GetLocalValues() {
            return new ReadOnlyEnumerable<KeyValuePair<IProperty, object>>(_values);
        }
        #endregion

       #region Open
        private volatile bool _isOpened;
        private Exception _openDataException;

        internal void OpenData() {
            lock (this) {
                if (!_isOpened) {
                    //数据无效，永远不能使用。
                    if (_openDataException != null) {
                        throw _openDataException;
                    }

                    try {
                        this.OpenDataCore();
                        _isOpened = true;
                    }
                    catch (Exception ex) {
                        //当展开失败后，此对象将永远不能使用。
                        _openDataException = ex;
                        throw;   
                    }
                }
            }
        }

        /// <summary>
        /// 返回此对象是否已经完成解开数据包工作。
        /// </summary>
        internal bool IsOpened {
            get {
                return this._isOpened;
            }
        }

        /// <summary>
        /// 派生类重载此方法，将原始的数据解开填充到当前数据包中。
        /// </summary>
        protected abstract void OpenDataCore();
        #endregion
    }
}
