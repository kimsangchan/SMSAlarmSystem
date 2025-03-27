using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMSAlarmSystem.Core.Interfaces
{
    // SMS 발송 서비스 인터페이스
    public interface ISMSService
    {
        // 단일 메시지 발송
        Task<bool> SendSMSAsync(string phoneNumber, string message);

        // 다수 수신자에게 메시지 발송
        Task<Dictionary<string, bool>> SendBulkSMSAsync(IEnumerable<string> phoneNumbers, string message);
    }
}
