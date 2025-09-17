using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edu_mobileapp.Models;

public class Asset
{
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string Image { get; set; } = "";   // Resources/Images dosya adı
    public int Quantity { get; set; }         // 1–7
    public string Location { get; set; } = "";
    public string Status { get; set; } = "Kullanımda"; // Kullanımda | Bakımda | Yedekte | Hurda

    // Yeni:
    public string Brand { get; set; } = "";   // Marka / model
    public string Spec { get; set; } = "";   // Güç / teknik özellik
}
