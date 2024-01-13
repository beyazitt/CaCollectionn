using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnionBase.Application.Services
{
    public interface IQRCodeService
    {
        byte[] GenerateQrCode(string text);
    }
}
