using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickType.UI;

namespace QuickType
{
    public class Bootstrapper
    {
        public static void Start<TViewModel, TPresenter>(IView data)
        where TViewModel : new()
        where TPresenter : PresenterBase<IView,TViewModel>
        {
            data.DataContext = new TViewModel();
            var presenter = (TPresenter)Activator.CreateInstance(typeof(TPresenter), data, (TViewModel)data.DataContext);
            data.OnLoaded += () => presenter.OnInitialize();
        }
    }
}
