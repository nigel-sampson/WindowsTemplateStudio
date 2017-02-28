﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Microsoft.Templates.Wizard.Host;
using System.Collections.ObjectModel;
using Microsoft.Templates.Wizard.Steps.Pages;
using Microsoft.Templates.Core;
using System.Windows.Input;
using Microsoft.Templates.Wizard.Dialog;

namespace Microsoft.Templates.Wizard.Steps.ConsumerFeatures
{
    public class ViewModel : StepViewModel
    {
        public ViewModel(WizardContext context) : base(context)
        {
        }

        public ICommand AddPageCommand => new RelayCommand(ShowAddFeatureDialog);
        public ICommand RemovePageCommand => new RelayCommand(RemoveFeature);

        public ObservableCollection<PageViewModel> Templates { get; } = new ObservableCollection<PageViewModel>();

        private PageViewModel _templateSelected;
        public PageViewModel TemplateSelected
        {
            get { return _templateSelected; }
            set
            {
                SetProperty(ref _templateSelected, value);
            }
        }

        public override string PageTitle => Strings.PageTitle;

        public override async Task InitializeAsync()
        {
            Templates.Clear();

            if (Context.State.ConsumerFeatures == null || Context.State.ConsumerFeatures.Count == 0)
            {
                AddFromLayout();
            }
            else
            {
                var selectedFeatures = Context.State.ConsumerFeatures
                                                        .Select(p => new PageViewModel(p.name, p.templateName));

                Templates.AddRange(selectedFeatures);
            }

            await Task.FromResult(true);
        }

        public override void SaveState()
        {
            Context.State.ConsumerFeatures.Clear();
            Context.State.ConsumerFeatures.AddRange(Templates.Select(t => (t.Name, t.TemplateName)));
        }

        public override void CleanState()
        {
            Context.State.ConsumerFeatures.Clear();
        }

        protected override Page GetPageInternal()
        {
            return new View();
        }

        private void AddFromLayout()
        {
            var projectTemplate = Context.TemplatesRepository.Find(t => t.GetProjectType() == Context.State.ProjectType && t.GetFrameworkList().Any(f => f == Context.State.Framework));
            var layout = projectTemplate.GetLayout();

            foreach (var item in layout)
            {
                var template = Context.TemplatesRepository.Find(t => t.Identity == item.templateIdentity);
                if (template != null && template.GetTemplateType() == TemplateType.ConsumerFeature)
                {
                    Templates.Add(new PageViewModel(item.name, template.Name, item.@readonly));
                }
            }
        }

        private void ShowAddFeatureDialog()
        {
            var dialog = new NewConsumerFeature.NewConsumerFeatureDialog(Context, Templates);
            var dialogResult = dialog.ShowDialog();

            if (dialogResult.HasValue && dialogResult.Value)
            {
                Templates.Add(new PageViewModel(dialog.Result.name, dialog.Result.templateName));
            }
        }

        private void RemoveFeature()
        {
            if (TemplateSelected != null && !TemplateSelected.Readonly)
            {
                Templates.Remove(TemplateSelected);
            }
        }
    }
}