using OnionBase.Application.Services;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnionBase.InfraStructure.Services
{
    public class QRCodeService : IQRCodeService
    {
        public byte[] GenerateQrCode(string text)
        {
            QRCodeGenerator generator = new();
            QRCodeData data = generator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qRCode = new(data);
            byte[] byteGraphic = qRCode.GetGraphic(10, new byte[] { 214, 72, 141 }, new byte[] { 255, 255, 255 });
            return byteGraphic;
        }
    }
}
