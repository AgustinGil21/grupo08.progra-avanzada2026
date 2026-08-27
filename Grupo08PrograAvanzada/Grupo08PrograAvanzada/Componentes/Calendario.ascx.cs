using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Grupo08PrograAvanzada.Componentes
{
    public partial class Calendario : UserControl
    {
        public event EventHandler DateSelected;

        public DateTime SelectedDate
        {
            get { return calendarControl.SelectedDate; }
            set { calendarControl.SelectedDate = value; }
        }

        public DateTime VisibleDate
        {
            get { return calendarControl.VisibleDate; }
            set { calendarControl.VisibleDate = value; }
        }

        public bool Visible
        {
            get { return base.Visible; }
            set { base.Visible = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                calendarControl.SelectedDate = DateTime.Now;
                calendarControl.VisibleDate = DateTime.Now;
            }
        }

        protected void calendarControl_SelectionChanged(object sender, EventArgs e)
        {
            // Dispara evento cuando se selecciona una fecha
            DateSelected?.Invoke(this, e);
        }
    }
}