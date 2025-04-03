// 작성자: Sangchan, Kim
// 작성일: 2025-03-31
// 기능: 알람 관련 비즈니스 로직을 처리하는 서비스
// 설명: 알람 포인트 관리 및 알람 트리거 기능을 제공합니다.

using Microsoft.Extensions.Logging;
using SMSAlarmSystem.Core.Interfaces;
using SMSAlarmSystem.Core.Models;
using SMSAlarmSystem.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMSAlarmSystem.Services.Services
{
    /// <summary>
    /// 알람 관련 비즈니스 로직을 처리하는 서비스 클래스
    /// 알람 포인트 관리 및 알람 트리거 기능을 제공합니다.
    /// </summary>
    public class AlarmService : IAlarmService
    {
        // 알람 포인트 데이터 접근을 위한 리포지토리
        private readonly IAlarmPointRepository _alarmPointRepository;

        // 메시지 그룹 데이터 접근을 위한 리포지토리
        private readonly IMessageGroupRepository _messageGroupRepository;

        // 회원 데이터 접근을 위한 리포지토리
        private readonly IMemberRepository _memberRepository;

        // 메시지 데이터 접근을 위한 리포지토리
        private readonly IMessageRepository _messageRepository;

        // SMS 발송 서비스
        private readonly ISMSService _smsService;

        // 로깅을 위한 로거 인스턴스
        private readonly ILogger<AlarmService> _logger;

        /// <summary>
        /// AlarmService 생성자
        /// </summary>
        /// <param name="alarmPointRepository">알람 포인트 리포지토리</param>
        /// <param name="messageGroupRepository">메시지 그룹 리포지토리</param>
        /// <param name="memberRepository">회원 리포지토리</param>
        /// <param name="messageRepository">메시지 리포지토리</param>
        /// <param name="smsService">SMS 발송 서비스</param>
        /// <param name="logger">로깅을 위한 ILogger 인스턴스</param>
        /// <exception cref="ArgumentNullException">필수 매개변수가 null인 경우 발생</exception>
        public AlarmService(
            IAlarmPointRepository alarmPointRepository,
            IMessageGroupRepository messageGroupRepository,
            IMemberRepository memberRepository,
            IMessageRepository messageRepository,
            ISMSService smsService,
            ILogger<AlarmService> logger)
        {
            // null 체크 및 예외 처리
            _alarmPointRepository = alarmPointRepository ?? throw new ArgumentNullException(nameof(alarmPointRepository), "알람 포인트 리포지토리는 null이 될 수 없습니다.");
            _messageGroupRepository = messageGroupRepository ?? throw new ArgumentNullException(nameof(messageGroupRepository), "메시지 그룹 리포지토리는 null이 될 수 없습니다.");
            _memberRepository = memberRepository ?? throw new ArgumentNullException(nameof(memberRepository), "회원 리포지토리는 null이 될 수 없습니다.");
            _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository), "메시지 리포지토리는 null이 될 수 없습니다.");
            _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService), "SMS 서비스는 null이 될 수 없습니다.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "로거는 null이 될 수 없습니다.");

            _logger.LogInformation("AlarmService 초기화 완료");
        }

        /// <summary>
        /// 모든 알람 포인트를 조회합니다.
        /// </summary>
        /// <returns>알람 포인트 목록 또는 오류 발생 시 빈 목록</returns>
        public async Task<IEnumerable<AlarmPoint>> GetAllAlarmPointsAsync()
        {
            try
            {
                _logger.LogInformation("모든 알람 포인트 조회 시작");
                var alarmPoints = await _alarmPointRepository.GetAllAsync();

                // null 체크 (방어적 프로그래밍)
                if (alarmPoints == null)
                {
                    _logger.LogWarning("리포지토리에서 null 반환됨. 빈 목록으로 대체합니다.");
                    return new List<AlarmPoint>();
                }

                _logger.LogInformation("모든 알람 포인트 조회 완료: {Count}개 조회됨", alarmPoints.Count());
                return alarmPoints;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "모든 알람 포인트 조회 중 오류 발생: {ErrorMessage}", ex.Message);
                return new List<AlarmPoint>();
            }
        }

        /// <summary>
        /// ID로 특정 알람 포인트를 조회합니다.
        /// </summary>
        /// <param name="id">조회할 알람 포인트 ID</param>
        /// <returns>조회된 알람 포인트 또는 null(알람 포인트가 없거나 오류 발생 시)</returns>
        public async Task<AlarmPoint?> GetAlarmPointByIdAsync(int id)
        {
            // ID 유효성 검사
            if (id <= 0)
            {
                _logger.LogWarning("알람 포인트 조회 실패: 유효하지 않은 ID={Id}", id);
                return null;
            }

            try
            {
                _logger.LogInformation("알람 포인트 조회 시작: ID={Id}", id);
                var alarmPoint = await _alarmPointRepository.GetByIdAsync(id);

                if (alarmPoint == null)
                {
                    _logger.LogWarning("알람 포인트를 찾을 수 없음: ID={Id}", id);
                }
                else
                {
                    _logger.LogInformation("알람 포인트 조회 완료: ID={Id}, 이름={Name}", id, alarmPoint.Name);
                }

                return alarmPoint;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "알람 포인트 조회 중 오류 발생: ID={Id}, 오류={ErrorMessage}", id, ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 활성화된 알람 포인트만 조회합니다.
        /// </summary>
        /// <returns>활성화된 알람 포인트 목록 또는 오류 발생 시 빈 목록</returns>
        public async Task<IEnumerable<AlarmPoint>> GetActiveAlarmPointsAsync()
        {
            try
            {
                _logger.LogInformation("활성화된 알람 포인트 조회 시작");
                var alarmPoints = await _alarmPointRepository.GetActiveAlarmPointsAsync();

                // null 체크 (방어적 프로그래밍)
                if (alarmPoints == null)
                {
                    _logger.LogWarning("리포지토리에서 null 반환됨. 빈 목록으로 대체합니다.");
                    return new List<AlarmPoint>();
                }

                _logger.LogInformation("활성화된 알람 포인트 조회 완료: {Count}개 조회됨", alarmPoints.Count());
                return alarmPoints;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "활성화된 알람 포인트 조회 중 오류 발생: {ErrorMessage}", ex.Message);
                return new List<AlarmPoint>();
            }
        }

        /// <summary>
        /// 특정 그룹에 연결된 알람 포인트를 조회합니다.
        /// </summary>
        /// <param name="groupId">조회할 그룹 ID</param>
        /// <returns>해당 그룹에 연결된 알람 포인트 목록 또는 오류 발생 시 빈 목록</returns>
        public async Task<IEnumerable<AlarmPoint>> GetAlarmPointsByGroupIdAsync(int groupId)
        {
            // ID 유효성 검사
            if (groupId <= 0)
            {
                _logger.LogWarning("그룹별 알람 포인트 조회 실패: 유효하지 않은 그룹 ID={GroupId}", groupId);
                return new List<AlarmPoint>();
            }

            try
            {
                _logger.LogInformation("그룹별 알람 포인트 조회 시작: 그룹ID={GroupId}", groupId);
                var alarmPoints = await _alarmPointRepository.GetAlarmPointsByGroupIdAsync(groupId);

                // null 체크 (방어적 프로그래밍)
                if (alarmPoints == null)
                {
                    _logger.LogWarning("리포지토리에서 null 반환됨. 빈 목록으로 대체합니다.");
                    return new List<AlarmPoint>();
                }

                _logger.LogInformation("그룹별 알람 포인트 조회 완료: 그룹ID={GroupId}, {Count}개 조회됨", groupId, alarmPoints.Count());
                return alarmPoints;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "그룹별 알람 포인트 조회 중 오류 발생: 그룹ID={GroupId}, 오류={ErrorMessage}", groupId, ex.Message);
                return new List<AlarmPoint>();
            }
        }

        /// <summary>
        /// 새 알람 포인트를 추가합니다.
        /// </summary>
        /// <param name="alarmPoint">추가할 알람 포인트 객체</param>
        /// <returns>추가 성공 여부</returns>
        public async Task<bool> AddAlarmPointAsync(AlarmPoint alarmPoint)
        {
            // null 체크
            if (alarmPoint == null)
            {
                _logger.LogError("알람 포인트 추가 실패: 알람 포인트가 null입니다.");
                return false;
            }

            // 필수 필드 유효성 검사
            if (string.IsNullOrWhiteSpace(alarmPoint.Name))
            {
                _logger.LogError("알람 포인트 추가 실패: 이름이 비어 있습니다.");
                return false;
            }

            if (alarmPoint.MessageGroupId <= 0)
            {
                _logger.LogError("알람 포인트 추가 실패: 유효하지 않은 메시지 그룹 ID={MessageGroupId}", alarmPoint.MessageGroupId);
                return false;
            }

            try
            {
                _logger.LogInformation("알람 포인트 추가 시작: 이름={Name}", alarmPoint.Name);
                await _alarmPointRepository.AddAsync(alarmPoint);
                _logger.LogInformation("알람 포인트 추가 완료: ID={Id}, 이름={Name}", alarmPoint.Id, alarmPoint.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "알람 포인트 추가 중 오류 발생: 이름={Name}, 오류={ErrorMessage}", alarmPoint.Name, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 기존 알람 포인트를 업데이트합니다.
        /// </summary>
        /// <param name="alarmPoint">업데이트할 알람 포인트 객체</param>
        /// <returns>업데이트 성공 여부</returns>
        public async Task<bool> UpdateAlarmPointAsync(AlarmPoint alarmPoint)
        {
            // null 체크
            if (alarmPoint == null)
            {
                _logger.LogError("알람 포인트 업데이트 실패: 알람 포인트가 null입니다.");
                return false;
            }

            // ID 유효성 검사
            if (alarmPoint.Id <= 0)
            {
                _logger.LogError("알람 포인트 업데이트 실패: 유효하지 않은 ID={Id}", alarmPoint.Id);
                return false;
            }

            // 필수 필드 유효성 검사
            if (string.IsNullOrWhiteSpace(alarmPoint.Name))
            {
                _logger.LogError("알람 포인트 업데이트 실패: 이름이 비어 있습니다. ID={Id}", alarmPoint.Id);
                return false;
            }

            if (alarmPoint.MessageGroupId <= 0)
            {
                _logger.LogError("알람 포인트 업데이트 실패: 유효하지 않은 메시지 그룹 ID={MessageGroupId}, 알람 포인트 ID={Id}",
                    alarmPoint.MessageGroupId, alarmPoint.Id);
                return false;
            }

            try
            {
                // 알람 포인트 존재 여부 확인
                var existingAlarmPoint = await _alarmPointRepository.GetByIdAsync(alarmPoint.Id);
                if (existingAlarmPoint == null)
                {
                    _logger.LogWarning("알람 포인트 업데이트 실패: 알람 포인트를 찾을 수 없음, ID={Id}", alarmPoint.Id);
                    return false;
                }

                _logger.LogInformation("알람 포인트 업데이트 시작: ID={Id}, 이름={Name}", alarmPoint.Id, alarmPoint.Name);
                await _alarmPointRepository.UpdateAsync(alarmPoint);
                _logger.LogInformation("알람 포인트 업데이트 완료: ID={Id}, 이름={Name}", alarmPoint.Id, alarmPoint.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "알람 포인트 업데이트 중 오류 발생: ID={Id}, 이름={Name}, 오류={ErrorMessage}",
                    alarmPoint.Id, alarmPoint.Name, ex.Message);
                return false;
            }
        }

        // 작성자: Sangchan, Kim
        // 작성일: 2025-04-03
        // 기능: 알람 포인트 삭제 서비스
        // 설명: 지정된 ID의 알람 포인트를 삭제하는 서비스 메서드입니다.

        /// <summary>
        /// 알람 포인트를 삭제합니다.
        /// </summary>
        /// <param name="id">삭제할 알람 포인트 ID</param>
        /// <returns>삭제 성공 여부</returns>
        public async Task<bool> DeleteAlarmPointAsync(int id)
        {
            // ID 유효성 검사
            if (id <= 0)
            {
                _logger.LogError("알람 포인트 삭제 실패: 유효하지 않은 ID={Id}", id);
                return false;
            }

            try
            {
                _logger.LogInformation("알람 포인트 삭제 시작: ID={Id}", id);

                // 알람 포인트 존재 여부 확인
                var alarmPoint = await _alarmPointRepository.GetByIdAsync(id);
                if (alarmPoint == null)
                {
                    _logger.LogWarning("알람 포인트 삭제 실패: 알람 포인트를 찾을 수 없음, ID={Id}", id);
                    return false;
                }

                // ID를 사용하여 알람 포인트 삭제 (AlarmPoint 객체 대신 ID 전달)
                await _alarmPointRepository.DeleteAsync(id);
                _logger.LogInformation("알람 포인트 삭제 완료: ID={Id}, 이름={Name}", id, alarmPoint.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "알람 포인트 삭제 중 오류 발생: ID={Id}, 오류={ErrorMessage}", id, ex.Message);
                return false;
            }
        }


        /// <summary>
        /// 알람을 발생시키고 관련 그룹에 메시지를 발송합니다.
        /// </summary>
        /// <param name="alarmPointId">알람을 발생시킬 알람 포인트 ID</param>
        /// <param name="messageContent">발송할 메시지 내용</param>
        /// <returns>알람 트리거 성공 여부</returns>
        public async Task<bool> TriggerAlarmAsync(int alarmPointId, string messageContent)
        {
            // ID 유효성 검사
            if (alarmPointId <= 0)
            {
                _logger.LogError("알람 트리거 실패: 유효하지 않은 알람 포인트 ID={AlarmPointId}", alarmPointId);
                return false;
            }

            // 메시지 내용 유효성 검사
            if (string.IsNullOrEmpty(messageContent))
            {
                _logger.LogError("알람 트리거 실패: 메시지 내용이 null 또는 빈 문자열입니다.");
                return false;
            }

            try
            {
                _logger.LogInformation("알람 트리거 시작: 알람포인트ID={AlarmPointId}", alarmPointId);

                // 알람 포인트 정보 가져오기
                var alarmPoint = await _alarmPointRepository.GetByIdAsync(alarmPointId);
                if (alarmPoint == null)
                {
                    _logger.LogWarning("알람 트리거 실패: 알람 포인트를 찾을 수 없음, ID={AlarmPointId}", alarmPointId);
                    return false;
                }

                // 알람 포인트 활성화 여부 확인
                if (!alarmPoint.IsActive)
                {
                    _logger.LogWarning("알람 트리거 실패: 알람 포인트가 비활성 상태임, ID={AlarmPointId}, 이름={Name}", alarmPointId, alarmPoint.Name);
                    return false;
                }

                // 연결된 메시지 그룹 가져오기
                var messageGroup = await _messageGroupRepository.GetByIdAsync(alarmPoint.MessageGroupId);
                if (messageGroup == null)
                {
                    _logger.LogWarning("알람 트리거 실패: 메시지 그룹을 찾을 수 없음, ID={MessageGroupId}", alarmPoint.MessageGroupId);
                    return false;
                }

                // 메시지 그룹 활성화 여부 확인
                if (!messageGroup.IsActive)
                {
                    _logger.LogWarning("알람 트리거 실패: 메시지 그룹이 비활성 상태임, ID={MessageGroupId}, 이름={Name}", messageGroup.Id, messageGroup.Name);
                    return false;
                }

                // 그룹에 속한 회원들 가져오기
                var members = await _memberRepository.GetMembersByGroupIdAsync(messageGroup.Id);

                // null 체크 (방어적 프로그래밍)
                if (members == null)
                {
                    _logger.LogWarning("알람 트리거 실패: 회원 목록이 null입니다. 그룹ID={MessageGroupId}", messageGroup.Id);
                    return false;
                }

                // 활성 상태인 회원만 필터링
                var activeMembers = members.Where(m => m.IsActive).ToList();

                // 활성 회원이 있는지 확인
                if (!activeMembers.Any())
                {
                    _logger.LogWarning("알람 트리거 실패: 활성 상태인 회원이 없음, 그룹ID={MessageGroupId}, 그룹명={GroupName}",
                        messageGroup.Id, messageGroup.Name);
                    return false;
                }

                // 메시지 발송 기록 저장
                var message = new Message
                {
                    Content = messageContent,
                    SendTime = DateTime.Now,
                    AlarmPointId = alarmPointId,
                    MessageGroupId = messageGroup.Id,
                    Status = "발송 중",
                    CreatedAt = DateTime.UtcNow
                };

                await _messageRepository.AddAsync(message);
                _logger.LogInformation("메시지 발송 기록 생성: ID={MessageId}", message.Id);

                // 회원들에게 SMS 발송
                var phoneNumbers = activeMembers.Select(m => m.PhoneNumber).ToList();
                _logger.LogInformation("SMS 발송 시작: 수신자 수={RecipientCount}", phoneNumbers.Count);

                var results = await _smsService.SendBulkSMSAsync(phoneNumbers, messageContent);

                // 발송 결과 업데이트
                var successCount = results.Count(r => r.Value);
                var totalCount = results.Count;

                message.Status = successCount == totalCount ? "발송 완료" :
                                successCount == 0 ? "발송 실패" : "일부 발송 실패";
                message.UpdatedAt = DateTime.UtcNow;
                await _messageRepository.UpdateAsync(message);

                _logger.LogInformation("알람 트리거 완료: 알람포인트ID={AlarmPointId}, 성공={SuccessCount}/{TotalCount}",
                    alarmPointId, successCount, totalCount);

                return successCount > 0; // 하나 이상 성공했으면 true 반환
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "알람 트리거 중 오류 발생: 알람포인트ID={AlarmPointId}, 오류={ErrorMessage}",
                    alarmPointId, ex.Message);
                return false;
            }
        }
    }
}

