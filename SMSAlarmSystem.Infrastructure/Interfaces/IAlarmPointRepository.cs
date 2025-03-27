using SMSAlarmSystem.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMSAlarmSystem.Core.Interfaces
{
    // 알람 포인트 저장소 인터페이스
    public interface IAlarmPointRepository : IRepository<AlarmPoint>
    {
        // 활성화된 알람 포인트만 가져오기
        Task<IEnumerable<AlarmPoint>> GetActiveAlarmPointsAsync();

        // 특정 그룹에 연결된 알람 포인트 가져오기
        Task<IEnumerable<AlarmPoint>> GetAlarmPointsByGroupIdAsync(int groupId);
    }
}
