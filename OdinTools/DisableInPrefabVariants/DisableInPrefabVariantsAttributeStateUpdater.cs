using DisableInPrefabVariants;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
#if UNITY_EDITOR
[assembly: RegisterStateUpdater(typeof(DisableInPrefabVariantsAttributeStateUpdater))]

namespace DisableInPrefabVariants
{
    public class DisableInPrefabVariantsAttributeStateUpdater : AttributeStateUpdater<DisableInPrefabVariantsAttribute>
    {
        private bool _disable;

        protected override void Initialize()
        {
            var unityObjectTarget = Property.Tree.WeakTargets[0] as UnityEngine.Object;

            if (unityObjectTarget == null) return;

            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null)
            {
                unityObjectTarget = stage.openedFromInstanceObject;
            }
            
            var type = PrefabUtility.GetPrefabAssetType(unityObjectTarget);
            bool isPartOfVariant = PrefabUtility.IsPartOfVariantPrefab(unityObjectTarget);
            _disable = type == PrefabAssetType.Variant || isPartOfVariant;
        }

        public override void OnStateUpdate()
        {
            if (Property.State.Enabled && _disable)
            {
                Property.State.Enabled = false;
            }
        }
    }
}
#endif