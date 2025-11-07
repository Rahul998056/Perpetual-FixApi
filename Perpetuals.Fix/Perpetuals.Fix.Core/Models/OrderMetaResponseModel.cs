using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Perpetuals.Fix.Core.Models
{
    public class OrderMetaResponseModel
    {
        public ResponseData response { get; set; }
        public bool success { get; set; }
    }

    public class ResponseData
    {
        public string base_currency { get; set; }
        public bool book_or_cancel { get; set; }
        public string cancel_reason { get; set; }
        public string canceled { get; set; }
        public string client_ref { get; set; }
        public string created { get; set; }
        public double created_timestamp { get; set; }
        public string direction { get; set; }
        public string em_uuid { get; set; }
        public string engine_id { get; set; }
        public string estim_market_value { get; set; }
        public string exec_vol { get; set; }
        public string external_ref { get; set; }
        public bool fill_or_kill { get; set; }
        public string fill_percent { get; set; }
        public bool immediate_or_cancel { get; set; }
        public bool is_triggered { get; set; }
        public int leverage { get; set; }
        public string limit_price { get; set; }
        public string liquidity_pool { get; set; }
        public string margin_blocked { get; set; }
        public string margin_call_trigger { get; set; }
        public string market { get; set; }
        public string on_stop_loss { get; set; }
        public string open_vol { get; set; }
        public string order_group { get; set; }
        public string order_sequence { get; set; }
        public string order_size { get; set; }
        public string order_type { get; set; }
        public string order_uuid { get; set; }
        public string quote_currency { get; set; }
        public string status { get; set; }
        public string symbol { get; set; }
        public string tip_quantity { get; set; }
        public Dictionary<string, object> trades { get; set; }  
        public string updated { get; set; }
        public string vault_uuid { get; set; }
    }
}
