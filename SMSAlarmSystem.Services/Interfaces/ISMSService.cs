using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// 작성자: Sangchan, Kim
// 작성일: 2025-03-31
// 기능: SMS 발송 서비스 인터페이스
// 설명: SMS 발송 기능을 정의하는 인터페이스로, 단일 및 대량 SMS 발송 메서드를 선언합니다.
namespace SMSAlarmSystem.Services.Interfaces
{
    /// <summary>
    /// SMS 메시지 발송 기능을 정의하는 인터페이스
    /// 이 인터페이스를 구현하는 클래스는 SMS 발송 기능을 제공해야 합니다.
    /// </summary>
    public interface ISMSService
    {
        /// <summary>
        /// 단일 수신자에게 SMS 메시지를 발송합니다.
        /// </summary>
        /// <param name="phoneNumber">수신자 전화번호 (E.164 형식 권장: +821012345678)</param>
        /// <param name="message">발송할 메시지 내용</param>
        /// <returns>발송 성공 여부 (true: 성공, false: 실패)</returns>
        Task<bool> SendSMSAsync(string phoneNumber, string message);

        /// <summary>
        /// 다수의 수신자에게 동일한 SMS 메시지를 발송합니다.
        /// </summary>
        /// <param name="phoneNumbers">수신자 전화번호 목록</param>
        /// <param name="message">발송할 메시지 내용</param>
        /// <returns>각 전화번호별 발송 결과를 담은 Dictionary (key: 전화번호, value: 발송 성공 여부)</returns>
        Task<Dictionary<string, bool>> SendBulkSMSAsync(IEnumerable<string> phoneNumbers, string message);
    }
}
