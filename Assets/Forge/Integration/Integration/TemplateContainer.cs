using Forge.Entities;
using UnityEngine;

namespace Forge.Unity {
    /// <summary>
    /// A BaseContainer type that contains ITemplate instances.
    /// </summary>
    public class TemplateContainer : BaseContainer<TemplateContainer, ITemplate> {
        public ITemplate Template {
            get;
            private set;
        }

        public override IQueryableEntity QueryableEntity {
            get { return Template; }
        }

        /// <summary>
        /// Creates the container game objects used for a nice editing experience for the level.
        /// </summary>
        /// <param name="templates">The templates to create containers for.</param>
        /// <param name="parent">The GameObject that will be the parent of all of the
        /// templates.</param>
        public static void CreateContainers(ITemplateGroup templates, GameObject parent) {
            foreach (ITemplate template in templates.Templates) {
                TemplateContainer.CreateTemplateContainer(template, parent);
            }
        }

        protected override int GetEntityId() {
            return Template.TemplateId;
        }

        protected override void NotifyLevelDesignerOfDestruction() {
            LevelDesigner.Instance.OnTemplateDestroyed(Template);
        }

        protected override void Initialize(ITemplate template) {
            Template = template;
        }

        public static TemplateContainer CreateTemplateContainer(ITemplate template, GameObject parent) {
            return CreateContainer(template, template.TemplateId, parent);
        }

        public static TemplateContainer GetContainer(ITemplate template) {
            return GetContainerById(template.TemplateId);
        }
    }
}