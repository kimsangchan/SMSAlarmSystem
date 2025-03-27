using SMSAlarmSystem.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMSAlarmSystem.Core.Interfaces
{
    // 메시지 저장소 인터페이스
    public interface IMessageRepository : IRepository<Message>
    {
        // 특정 기간 내 발송된 메시지 가져오기
        Task<IEnumerable<Message>> GetMessagesByDateRangeAsync(DateTime start, DateTime end);

        // 특정 그룹에 발송된 메시지 가져오기
        Task<IEnumerable<Message>> GetMessagesByGroupIdAsync(int groupId);

        // 특정 알람 포인트에서 발생한 메시지 가져오기
        Task<IEnumerable<Message>> GetMessagesByAlarmPointIdAsync(int alarmPointId);
    }
}
