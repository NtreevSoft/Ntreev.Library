﻿//Released under the MIT License.
//
//Copyright (c) 2018 Ntreev Soft co., Ltd.
//
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
//documentation files (the "Software"), to deal in the Software without restriction, including without limitation the 
//rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit 
//persons to whom the Software is furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the 
//Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
//WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
//COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
//OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using Ntreev.Library;
using Ntreev.Library.IO;
using Ntreev.Library.Linq;
using Ntreev.Library.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Ntreev.Library.ObjectModel
{
    public class CategoryBase<_I, _C, _IC, _CC, _CT> : IItemAttributeProvider, IItem
        where _I : ItemBase<_I, _C, _IC, _CC, _CT>
        where _C : CategoryBase<_I, _C, _IC, _CC, _CT>, new()
        where _IC : ItemContainer<_I, _C, _IC, _CC, _CT>, new()
        where _CC : CategoryContainer<_I, _C, _IC, _CC, _CT>, new()
        where _CT : ItemContext<_I, _C, _IC, _CC, _CT>
    {
        private string name;
        private string path;
        internal _C parent;
        private readonly ItemCollection items;
        private readonly CategoryCollection categories;
        private readonly ChildCollection childs;
        private _CC container;
        private int depth;
        private ItemAttributes itemAttributes;
        private bool isDisposed;
        private bool isDisposing;
        private PropertyCollection extendedProperties;

        public CategoryBase()
        {
            this.items = new ItemCollection(this);
            this.categories = new CategoryCollection(this);
            this.childs = new ChildCollection(this);
        }

        internal CategoryBase(ItemAttributes itemAttributes)
            : this()
        {
            this.itemAttributes = itemAttributes;
        }

        public void Dispose()
        {
            this.ValidateDelete();

            var categoryPath = this.Path;
            var container = this.container;

            this.isDisposing = true;
            this.OnDeleted(EventArgs.Empty);
            this.isDisposing = false;
            this.isDisposed = true;

            if (container != null)
                container.InvokeCategoryDeleted(categoryPath, this as _C);
        }

        public override string ToString()
        {
            return this.Key;
        }

        public int CompareTo(object obj)
        {
            var dest = obj as _C;

            if (this.Parent == dest.Parent)
                return this.Path.CompareTo(dest.Path);
            if (this.depth == dest.depth)
                return (this.Parent as IComparable).CompareTo(dest.Parent);
            return this.depth.CompareTo(dest.depth);
        }

        public void ValidateRename(string newName)
        {
            if (newName == null)
                throw new ArgumentNullException(nameof(newName));
            if (this.ItemAttributes.HasFlag(ItemAttributes.UniqueName) == true)
                throw new ArgumentException(Resources.Exception_UniqueObjectCannotRename);
            if (NameValidator.VerifyName(newName) == false)
                throw new ArgumentException(string.Format(Resources.Exception_InvalidName_Format, newName), nameof(newName));

            if (this.Container != null)
            {
                var newPath = CategoryBase<_I, _C, _IC, _CC, _CT>.CreatePath(newName, this.Parent);
                if (this.Container.Contains(newPath) == true && this.Container[newPath] != this)
                    throw new ArgumentException(string.Format(Resources.Exception_AlreadyExistedItem_Format, newName), nameof(newName));
            }

            var parent = this.parent;
            if (parent != null)
            {
                if (parent.Categories.ContainsKey(newName) == true)
                    throw new ArgumentException(string.Format(Resources.Exception_AlreadyExistedItem_Format, newName));
                if (parent.items.ContainsKey(newName) == true)
                    throw new ArgumentException(string.Format(Resources.Exception_AlreadyExistedItem_Format, newName));
                this.ValidateMove(parent);
            }
        }

        public void ValidateMove(_C parent)
        {
            if (parent == null)
                return;

            if (parent == this)
                throw new ArgumentException(Resources.Exception_CannotBeSetItSelfAsParent, nameof(parent));

            if (this.name == null)
                throw new InvalidOperationException(Resources.Exception_UnnamedFolderCannotHaveParent);

            if (EnumerableUtility.Descendants(this, item => item.Categories).Contains(parent) == true)
                throw new ArgumentException(Resources.Exception_ChildFolderCannotBeSetAsParent, nameof(parent));

            if (parent.Categories.ContainsKey(this.Name) == true && parent.categories[this.Name] != this)
                throw new ArgumentException(Resources.Exception_SameFolderInParent, nameof(parent));

            if (parent.Items.ContainsKey(this.Name) == true)
                throw new ArgumentException(Resources.Exception_SameItemInParent, nameof(parent));
        }

        public void ValidateDelete()
        {
            if (this.isDisposed == true)
                throw new ObjectDisposedException(this.name);

            if (this.ItemAttributes.HasFlag(ItemAttributes.Indestructible) == true)
                throw new InvalidOperationException(Resources.Exception_IndestructibleObject);
        }

        public string MakeRelative(string path)
        {
            var relative = new Uri(this.Path).MakeRelativeUri(new Uri(path));
            return relative.ToString();
        }

        public ItemCollection Items
        {
            get { return this.items; }
        }

        public CategoryCollection Categories
        {
            get { return this.categories; }
        }

        public _C Parent
        {
            get { return this.parent; }
            set
            {
                if (this.parent == value)
                    return;

                if (this.isDisposing == true)
                {
                    if (this.parent != null)
                    {
                        this.parent.categories.Remove(this as _C);
                        this.parent.childs.Remove(this);
                    }
                    this.parent = null;
                    return;
                }

                this.ValidateMove(value);

                var oldParent = this.Parent;
                var container = this.container;
                var oldPath = this.Path;

                if (this.parent != null)
                {
                    this.parent.categories.Remove(this as _C);
                    this.parent.childs.Remove(this);
                }
                this.parent = value;
                this.path = null;
                if (this.parent != null)
                {
                    this.parent.categories.Add(this as _C);
                    this.parent.childs.Add(this);
                }
                this.OnMoved(EventArgs.Empty);
                this.OnPathChanged(oldPath, this.Path);

                if (container != null)
                    container.InvokeCategoryMoved(this as _C, oldPath, oldParent?.Path);
            }
        }

        public string Name
        {
            get
            {
                if (this.name == null)
                    return string.Empty;
                return this.name;
            }
            set
            {
                if (this.name == value)
                    return;

                this.ValidateRename(value);

                var parent = this.parent;

                var oldName = this.Name;
                var oldPath = this.Path;
                var container = this.container;

                this.name = value;
                this.path = null;
                if (parent != null)
                {
                    parent.categories.Move(oldName, this as _C);
                    parent.childs.Move(oldName, this);
                }

                this.OnRenamed(EventArgs.Empty);
                this.OnPathChanged(oldPath, this.Path);

                if (container != null)
                    container.InvokeCategoryRenamed(this as _C, oldName, oldPath);
            }
        }

        public string Path
        {
            get
            {
                if (this.path == null)
                {
                    this.path = CategoryBase<_I, _C, _IC, _CC, _CT>.CreatePath(this.Name, this.Parent);
                }
                return this.path;
            }
        }

        public string Key
        {
            get { return this.Path; }
        }

        public bool IsDisposed
        {
            get { return this.isDisposed; }
            internal set { this.isDisposed = value; }
        }

        public int Depth
        {
            get { return this.depth; }
        }

        public virtual ItemAttributes ItemAttributes
        {
            get { return this.itemAttributes; }
        }

        [Browsable(false)]
        public PropertyCollection ExtendedProperties
        {
            get
            {
                if (this.extendedProperties == null)
                {
                    this.extendedProperties = new PropertyCollection();
                }
                return this.extendedProperties;
            }
        }

        public _CC Container
        {
            get { return this.container; }
            internal set
            {
                var isSame = this.container == value;
                this.container = value;
                this.depth = this.Parent == null ? 0 : this.Parent.Path.Where(item => item == PathUtility.SeparatorChar).Count() + 1;
                this.path = null;

                if (isSame == false)
                {
                    if (this.container == null)
                    {
                        this.OnDetached();
                    }
                    else
                    {
                        this.OnAttached();
                    }
                }
            }
        }

        public _CT Context
        {
            get
            {
                if (this.container == null)
                    return null;
                return this.container.Context;
            }
        }

        public event EventHandler Renamed;

        public event EventHandler Moved;

        public event EventHandler Deleted;

        internal void InvokePathChanged()
        {
            this.OnPathChanged(null, PathUtility.Separator);
        }

        internal ChildCollection Childs
        {
            get { return this.childs; }
        }

        protected virtual void OnRenamed(EventArgs e)
        {
            this.Renamed?.Invoke(this, e);
        }

        protected virtual void OnMoved(EventArgs e)
        {
            this.Moved?.Invoke(this, e);
        }

        protected virtual void OnDeleted(EventArgs e)
        {
            if (this.ItemAttributes.HasFlag(ItemAttributes.Indestructible) == true)
                throw new InvalidOperationException(Resources.Exception_IndestructibleObject);

            foreach (var item in this.Items.ToArray())
            {
                item.IsDisposed = true;
            }

            foreach (var item in this.Categories.ToArray())
            {
                item.IsDisposed = true;
            }

            this.Deleted?.Invoke(this, e);

            this.parent = null;
            this.container = null;
            this.depth = 0;
        }

        protected virtual void OnAttached()
        {

        }

        protected virtual void OnDetached()
        {

        }

        protected virtual void OnPathChanged(string oldPath, string newPath)
        {
            foreach (var item in this.Categories)
            {
                item.OnPathChanged(oldPath, newPath);
            }

            foreach (var item in this.Items)
            {
                item.InvokeOnPathChanged(oldPath, newPath);
            }
        }

        protected void SetItemAttributes(ItemAttributes flag, bool value)
        {
            flag = flag & ~ItemAttributes.Root;

            if (value == true)
            {
                this.itemAttributes |= flag;
            }
            else
            {
                this.itemAttributes &= ~flag;
            }
        }

        protected internal bool IsDisposing
        {
            get { return this.isDisposing; }
        }

        private static string CreatePath(string name, _C parent)
        {
            if (parent == null)
                return name + PathUtility.Separator;
            return parent.Path + name + PathUtility.Separator;
        }

        #region IItemAttributeProvider

        ItemAttributes IItemAttributeProvider.ItemAttributes
        {
            get { return this.itemAttributes; }
            set { this.itemAttributes = value; }
        }
        #endregion

        #region classes

        public class ItemCollection : ContainerBase<_I>
        {
            private readonly CategoryBase<_I, _C, _IC, _CC, _CT> category;

            public ItemCollection(CategoryBase<_I, _C, _IC, _CC, _CT> category)
            {
                this.category = category;
            }

            internal void Add(_I item)
            {
                if (item.IsDisposed == true)
                    throw new ObjectDisposedException(item.Name);

                this.AddBase(item.Name, item);
            }

            internal bool Remove(_I item)
            {
                return this.RemoveBase(item.Name);
            }

            internal void Move(string oldName, _I item)
            {
                this.ReplaceKeyBase(oldName, item.Name);
            }

            internal CategoryBase<_I, _C, _IC, _CC, _CT> Category
            {
                get { return this.category; }
            }

            public new _I this[string name]
            {
                get
                {
                    return base[name];
                }
            }
        }

        public class CategoryCollection : ContainerBase<_C>
        {
            private readonly CategoryBase<_I, _C, _IC, _CC, _CT> category;

            public CategoryCollection(CategoryBase<_I, _C, _IC, _CC, _CT> category)
            {
                this.category = category;
            }

            internal void Add(_C category)
            {
                if (category.IsDisposed == true)
                    throw new ObjectDisposedException(category.Name);
                this.AddBase(category.Name, category);
            }

            internal bool Remove(_C category)
            {
                return this.RemoveBase(category.Name);
            }

            internal void Move(string oldName, _C category)
            {
                this.ReplaceKeyBase(oldName, category.Name);
            }

            internal CategoryBase<_I, _C, _IC, _CC, _CT> Category
            {
                get { return this.category; }
            }

            public new _C this[string name]
            {
                get
                {
                    return base[name];
                }
            }
        }

        internal class ChildCollection : ContainerBase<IItem>
        {
            private readonly CategoryBase<_I, _C, _IC, _CC, _CT> category;

            public ChildCollection(CategoryBase<_I, _C, _IC, _CC, _CT> category)
            {
                this.category = category;
            }

            internal void Add(IItem category)
            {
                //if (category.IsDisposed == true)
                //    throw new ObjectDisposedException(category.Name);
                this.AddBase(category.Name, category);
            }

            internal bool Remove(IItem category)
            {
                return this.RemoveBase(category.Name);
            }

            internal void Move(string oldName, IItem category)
            {
                this.ReplaceKeyBase(oldName, category.Name);
            }
        }

        #endregion

        #region IItem

        IItem IItem.Parent
        {
            get { return this.Parent; }
        }

        IContainer<IItem> IItem.Childs
        {
            get { return this.childs; }
        }

        #endregion
    }
}
