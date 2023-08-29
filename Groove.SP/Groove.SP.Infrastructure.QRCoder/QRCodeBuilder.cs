using QRCoder;

namespace Groove.SP.Infrastructure.QRCoder
{
    public class QRCodeBuilder : IQRCodeBuilder
    {
        public QRCodeBuilder()
        {

        }

        public byte[] GenerateQRCode(string txtQRCode)
        {
            var qrGen = new QRCodeGenerator();
            var qrCode = qrGen.CreateQrCode(txtQRCode, QRCodeGenerator.ECCLevel.Q);
            var qrBmp = new BitmapByteQRCode(qrCode);
            return qrBmp.GetGraphic(20);
        }
    }
}