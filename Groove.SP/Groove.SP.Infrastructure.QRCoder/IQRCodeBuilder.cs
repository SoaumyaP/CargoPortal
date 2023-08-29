using System;

namespace Groove.SP.Infrastructure.QRCoder
{
    public interface IQRCodeBuilder
    {
        byte[] GenerateQRCode(string txtQRCode);
    }
}