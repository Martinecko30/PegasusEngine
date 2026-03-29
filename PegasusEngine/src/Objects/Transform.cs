using System.Collections;
using PegasusEngine.Core;
using PegasusEngine.Modules.Scripting;

namespace PegasusEngine.Objects;

[Serializable]
public class Transform : Behaviour, IEnumerable
{
    protected GameObject GameObject { get; private set; }

    private List<Transform> children = new();
    public IReadOnlyList<Transform> Children => children;
    public int ChildCount => children.Count;
    
    public Transform? Parent { get; private set; }
    
    public Transform() {}

    public Transform(GameObject gameObject, Transform? parent = null)
    {
        this.GameObject = gameObject;
        this.Parent = parent;
    }
    
    /// <summary>
    /// Safely changes the parent of this transform and automatically 
    /// updates the child lists of both the old and new parents.
    /// </summary>
    public void SetParent(Transform? newParent)
    {
        if (Parent == newParent)
            return;

        if (newParent == this)
        {
            Log.EngineWarn("SetParent: Cannot set a Transform's parent to itself!");
            return;
        }

        if (newParent != null && newParent.IsChildOf(this))
        {
            Log.EngineWarn($"SetParent: Cannot set '{newParent.GameObject.Name}' as a parent because it is already a child of '{this.GameObject.Name}'!");
            return;
        }

        if (Parent != null)
        {
            Parent.children.Remove(this);
        }

        Parent = newParent;

        if (Parent != null)
        {
            Parent.children.Add(this);
        }
        
        // TODO (Future): Recalculate Local/World Matrices here!
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