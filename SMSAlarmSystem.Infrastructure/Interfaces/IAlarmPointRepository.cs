// 작성자: Sangchan, Kim
// 작성일: 2025-03-31
// 기능: 알람 포인트 데이터 접근을 위한 인터페이스
// 설명: 알람 포인트 관련 데이터 접근 메서드를 정의합니다.

using SMSAlarmSystem.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMSAlarmSystem.Core.Interfaces
{
    /// <summary>
    /// 알람 포인트 데이터 접근을 위한 인터페이스
    /// 기본 CRUD 작업 외에 알람 포인트 관련 특수 조회 메서드를 정의합니다.
    /// </summary>
    public interface IAlarmPointRepository : IRepository<AlarmPoint>
    {
        /// <summary>
        /// 활성화된 알람 포인트만 조회합니다.
        /// </summary>
        /// <returns>활성화된 알람 포인트 목록</returns>
        Task<IEnumerable<AlarmPoint>> GetActiveAlarmPointsAsync();

        /// <summary>
        /// 특정 그룹에 연결된 알람 포인트를 조회합니다.
        /// </summary>
        /// <param name="groupId">조회할 그룹 ID</param>
        /// <returns>해당 그룹에 연결된 알람 포인트 목록</returns>
        Task<IEnumerable<AlarmPoint>> GetAlarmPointsByGroupIdAsync(int groupId);
    }
}
