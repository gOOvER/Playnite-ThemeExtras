using Extras.Models;
using Playnite.SDK.Controls;
using Playnite.SDK.Models;
using PlayniteCommon.Web;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Extras.Controls
{
    /// <summary>
    /// Interaktionslogik für Links.xaml
    /// </summary>
    public partial class Links : PluginUserControl
    {
        private readonly ObservableCollection<LinkExt> links = new ObservableCollection<LinkExt>();
        private Game subscribedGame = null;

        public Links()
        {
            InitializeComponent();
            LinksItemsControl.ItemsSource = links;
            Unloaded += Links_Unloaded;
        }

        private void Links_Unloaded(object sender, RoutedEventArgs e)
        {
            DetachGame(subscribedGame);
            subscribedGame = null;
        }

        private void DetachGame(Game game)
        {
            if (game != null)
            {
                game.PropertyChanged -= Game_PropertyChanged;
                if (game.Links is ObservableCollection<Link> oldLinks)
                {
                    oldLinks.CollectionChanged -= Links_CollectionChanged;
                }
            }
        }

        public override async void GameContextChanged(Game oldContext, Game newContext)
        {
            try
            {
                DetachGame(oldContext);
                subscribedGame = newContext;

                if (newContext is Game game)
                {
                    game.PropertyChanged += Game_PropertyChanged;
                    if (game.Links is ObservableCollection<Link> newLinks)
                    {
                        newLinks.CollectionChanged -= Links_CollectionChanged;
                        newLinks.CollectionChanged += Links_CollectionChanged;
                    }
                    await UpdateLinksAsync(game);
                }
                else
                {
                    if (Dispatcher.CheckAccess())
                    {
                        links.Clear();
                    }
                    else
                    {
                        await Dispatcher.InvokeAsync(() => links.Clear());
                    }
                }
            }
            catch (Exception ex)
            {
                ThemeExtras.logger.Error(ex, "Error in Links.GameContextChanged");
            }
        }

        private async Task UpdateLinksAsync(Game game)
        {
            if (game == null)
            {
                if (Dispatcher.CheckAccess())
                {
                    links.Clear();
                }
                else
                {
                    await Dispatcher.InvokeAsync(() => links.Clear());
                }
                return;
            }

            try
            {
                List<LinkExt> newItems = new List<LinkExt>();
                if (game.Links is ObservableCollection<Link> gameLinks)
                {
                    foreach (var l in gameLinks)
                    {
                        newItems.Add(new LinkExt(l));
                    }
                }

                if (Dispatcher.CheckAccess())
                {
                    links.Clear();
                    foreach (var item in newItems)
                    {
                        links.Add(item);
                    }
                }
                else
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        links.Clear();
                        foreach (var item in newItems)
                        {
                            links.Add(item);
                        }
                    });
                }

                foreach (var l in newItems)
                {
                    var icon = await LinkExt.GetIconAsync(l.Url);
                    if (Dispatcher.CheckAccess())
                    {
                        l.Icon = icon;
                    }
                    else
                    {
                        await Dispatcher.InvokeAsync(() => l.Icon = icon);
                    }
                }
            }
            catch (Exception ex)
            {
                ThemeExtras.logger.Debug(ex, $"Failed to update link icons for game {game.Name}");
            }
        }

        private async void Links_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                var game = subscribedGame;
                if (game != null)
                {
                    await UpdateLinksAsync(game);
                }
            }
            catch (Exception ex)
            {
                ThemeExtras.logger.Error(ex, "Error in Links_CollectionChanged");
            }
        }

        private async void Game_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                if (e.PropertyName == nameof(Game.Links))
                {
                    var game = sender as Game ?? subscribedGame;
                    if (game != null)
                    {
                        if (game.Links is ObservableCollection<Link> gameLinks)
                        {
                            gameLinks.CollectionChanged -= Links_CollectionChanged;
                            gameLinks.CollectionChanged += Links_CollectionChanged;
                        }
                        await UpdateLinksAsync(game);
                    }
                }
            }
            catch (Exception ex)
            {
                ThemeExtras.logger.Error(ex, "Error in Game_PropertyChanged for Links");
            }
        }
    }
}
