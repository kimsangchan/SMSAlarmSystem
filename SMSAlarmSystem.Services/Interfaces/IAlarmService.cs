// 작성자: Sangchan, Kim
// 작성일: 2025-03-31
// 기능: 알람 관련 비즈니스 로직을 정의하는 인터페이스
// 설명: 알람 포인트 관리 및 알람 트리거 기능에 대한 계약을 정의합니다.

using SMSAlarmSystem.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMSAlarmSystem.Services.Interfaces
{
    /// <summary>
    /// 알람 관련 비즈니스 로직을 정의하는 인터페이스
    /// </summary>
    public interface IAlarmService
    {
        /// <summary>
        /// 모든 알람 포인트를 조회합니다.
        /// </summary>
        /// <returns>알람 포인트 목록</returns>
        Task<IEnumerable<AlarmPoint>> GetAllAlarmPointsAsync();

        /// <summary>
        /// ID로 특정 알람 포인트를 조회합니다.
        /// </summary>
        /// <param name="id">조회할 알람 포인트 ID</param>
        /// <returns>조회된 알람 포인트 또는 null(알람 포인트가 없는 경우)</returns>
        Task<AlarmPoint?> GetAlarmPointByIdAsync(int id);

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

        /// <summary>
        /// 새 알람 포인트를 추가합니다.
        /// </summary>
        /// <param name="alarmPoint">추가할 알람 포인트 객체</param>
        /// <returns>추가 성공 여부</returns>
        Task<bool> AddAlarmPointAsync(AlarmPoint alarmPoint);

        /// <summary>
        /// 기존 알람 포인트를 업데이트합니다.
        /// </summary>
        /// <param name="alarmPoint">업데이트할 알람 포인트 객체</param>
        /// <returns>업데이트 성공 여부</returns>
        Task<bool> UpdateAlarmPointAsync(AlarmPoint alarmPoint);

        /// <summary>
        /// 알람 포인트를 삭제합니다.
        /// </summary>
        /// <param name="id">삭제할 알람 포인트 ID</param>
        /// <returns>삭제 성공 여부</returns>
        Task<bool> DeleteAlarmPointAsync(int id);

        /// <summary>
        /// 알람을 발생시키고 관련 그룹에 메시지를 발송합니다.
        /// </summary>
        /// <param name="alarmPointId">알람을 발생시킬 알람 포인트 ID</param>
        /// <param name="messageContent">발송할 메시지 내용</param>
        /// <returns>알람 트리거 성공 여부</returns>
        Task<bool> TriggerAlarmAsync(int alarmPointId, string messageContent);
    }
}
