using Pikl.Data;
using UnityEditor;

namespace Pikl.Editor.Inspectors {
    [CustomEditor( typeof( DropTable ) )]
    public class DropTableEditor : UnityEditor.Editor {
        DropTable m_Instance;
        PropertyField[] m_fields;
 
 
        public void OnEnable()
        {
            m_Instance = target as DropTable;
            m_fields = ExposeProperties.GetProperties( m_Instance );
        }
 
        public override void OnInspectorGUI () {
 
            if ( m_Instance == null )
                return;
 
            this.DrawDefaultInspector();
 
            ExposeProperties.Expose( m_fields );
 
        }
    }
}