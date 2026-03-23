using System.Collections;
using PegasusEngine.Modules.Scripting;
using PegasusEngine.Objects.Components;

namespace PegasusEngine.Objects;

public class Transform : Behaviour, IEnumerable
{
    protected GameObject GameObject { get; private set; }

    private List<Transform> children = new();
    public IReadOnlyList<Transform> Children => children;
    public int ChildCount => children.Count;
    
    public Transform? Parent { get; set; }

    public Transform(GameObject gameObject, Transform? parent = null)
    {
        this.GameObject = gameObject;
        this.Parent = parent;
    }
    
    public Transform GetChild(int index) => children[index];

    /// <summary>
    /// Searches for a child transform with the given name.
    /// </summary>
    /// <param name="name">Name of the child.</param>
    /// <returns>The child with the given name.</returns>
    public Transform? Find(string name)
    {
        foreach (var child in Children)
        {
            if (child.GameObject.Name.Equals(name))
                return child;
        }
        return null;
    }

    /// <summary>
    /// Returns true if this transform is a child of the given parent.
    /// </summary>
    /// <param name="parent">Suspected parent of this transform.</param>
    /// <returns>
    /// true - if this is the child of the suspected parent
    /// false - otherwise
    /// </returns>
    public bool IsChildOf(Transform parent)
    {
        if (Parent == null)
            return false;
        return Parent == parent || Parent.IsChildOf(parent);
    }


    public IEnumerator GetEnumerator()
    {
        return new Transform.Enumerator(this);
    }
    
    private class Enumerator : IEnumerator
    {
        Transform outer;
        int currentIndex = -1;

        internal Enumerator(Transform outer)
        {
            this.outer = outer;
        }

        //*undocumented*
        public object Current
        {
            get { return outer.GetChild(currentIndex); }
        }

        //*undocumented*
        public bool MoveNext()
        {
            int childCount = outer.ChildCount;
            return ++currentIndex < childCount;
        }

        //*undocumented*
        public void Reset() { currentIndex = -1; }
    }
}