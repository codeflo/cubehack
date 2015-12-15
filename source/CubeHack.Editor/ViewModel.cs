// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.EditorModel;
using CubeHack.FrontEnd;
using CubeHack.Game;
using CubeHack.Storage;
using CubeHack.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CubeHack.Editor
{
    public class ViewModel : NotifyPropertyChanged
    {
        private string _modName;

        private Item _modItem;

        private int _lockCount;

        public ViewModel()
        {
            SaveCommand = new DelegateCommand(Save);
            StartCommand = new DelegateCommand(Start);
            QuitCommand = new DelegateCommand(Quit);

            ModName = "Core";
            ModItem = Item.Create(typeof(CubeHack.Data.Mod));
            ModItem.IsExpanded = true;
            Load();
        }

        public event Action TriggerFocusChange;

        public ICommand SaveCommand { get; }

        public ICommand StartCommand { get; }

        public ICommand QuitCommand { get; }

        public bool IsEnabled => _lockCount == 0;

        public string ModName
        {
            get { return _modName; }
            set { SetAndNotify(ref _modName, value); }
        }

        public Item ModItem
        {
            get { return _modItem; }
            set { SetAndNotify(ref _modItem, value); }
        }

        private static string GetDirectory()
        {
            return Path.GetDirectoryName(typeof(ViewModel).Assembly.Location);
        }

        private void Quit()
        {
            Save();
            Application.Current.Shutdown();
        }

        private void Load()
        {
            string s = File.ReadAllText(GetPath(), Encoding.UTF8);
            var json = JToken.Parse(s);
            ModItem.Load(json);
        }

        private void Save()
        {
            /* Refocus, to propagate any changes to text boxes into the view model. */
            TriggerFocusChange?.Invoke();
            string s = JsonConvert.SerializeObject(ModItem.Save(), Formatting.Indented);
            File.WriteAllText(GetPath(), s, Encoding.UTF8);
        }

        private async void Start()
        {
            using (Lock())
            {
                Save();

                await Task.Run(
                    () =>
                    {
                        try
                        {
                            using (var universe = new Universe(NullSaveFile.Instance, DataLoader.LoadMod(_modName)))
                            {
                                using (var container = new DependencyInjectionContainer())
                                {
                                    var gameApp = container.Resolve<GameApp>();
                                    gameApp.Connect(universe.ConnectPlayer());
                                    gameApp.Run();
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                    });
            }
        }

        private string GetPath()
        {
            return Path.Combine(GetDirectory(), "Mods", _modName + ".cubemod.json");
        }

        private IDisposable Lock()
        {
            if (_lockCount++ == 0)
            {
                try
                {
                    Notify(nameof(IsEnabled));
                }
                catch
                {
                    Unlock();
                }
            }

            return new DelegateDisposable(Unlock);
        }

        private void Unlock()
        {
            if (--_lockCount == 0) Notify(nameof(IsEnabled));
        }
    }
}
