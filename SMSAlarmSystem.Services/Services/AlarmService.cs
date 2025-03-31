// 작성자: Sangchan, Kim
// 작성일: 2025-03-27
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
    public class AlarmService
    {
        private readonly IAlarmPointRepository _alarmPointRepository;
        private readonly IMessageGroupRepository _messageGroupRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly ISMSService _smsService;
        private readonly ILogger<AlarmService> _logger;

        // 생성자 - 필요한 저장소 및 서비스 주입
        public AlarmService(
            IAlarmPointRepository alarmPointRepository,
            IMessageGroupRepository messageGroupRepository,
            IMemberRepository memberRepository,
            IMessageRepository messageRepository,
            ISMSService smsService,
            ILogger<AlarmService> logger)
        {
            _alarmPointRepository = alarmPointRepository ?? throw new ArgumentNullException(nameof(alarmPointRepository));
            _messageGroupRepository = messageGroupRepository ?? throw new ArgumentNullException(nameof(messageGroupRepository));
            _memberRepository = memberRepository ?? throw new ArgumentNullException(nameof(memberRepository));
            _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
            _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // 모든 알람 포인트 가져오기
        public async Task<IEnumerable<AlarmPoint>> GetAllAlarmPointsAsync()
        {
            try
            {
                _logger.LogInformation("모든 알람 포인트 조회 시작");
                var alarmPoints = await _alarmPointRepository.GetAllAsync();
                _logger.LogInformation("모든 알람 포인트 조회 완료: {Count}개 조회됨", alarmPoints.Count());
                return alarmPoints;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "모든 알람 포인트 조회 중 오류 발생");
                return new List<AlarmPoint>();
            }
        }

        // ID로 알람 포인트 가져오기
        public async Task<AlarmPoint?> GetAlarmPointByIdAsync(int id)
        {
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
                _logger.LogError(ex, "알람 포인트 조회 중 오류 발생: ID={Id}", id);
                return null;
            }
        }

        // 활성화된 알람 포인트만 가져오기
        public async Task<IEnumerable<AlarmPoint>> GetActiveAlarmPointsAsync()
        {
            try
            {
                _logger.LogInformation("활성화된 알람 포인트 조회 시작");
                var alarmPoints = await _alarmPointRepository.GetActiveAlarmPointsAsync();
                _logger.LogInformation("활성화된 알람 포인트 조회 완료: {Count}개 조회됨", alarmPoints.Count());
                return alarmPoints;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "활성화된 알람 포인트 조회 중 오류 발생");
                return new List<AlarmPoint>();
            }
        }

        // 특정 그룹에 연결된 알람 포인트 가져오기
        public async Task<IEnumerable<AlarmPoint>> GetAlarmPointsByGroupIdAsync(int groupId)
        {
            try
            {
                _logger.LogInformation("그룹별 알람 포인트 조회 시작: 그룹ID={GroupId}", groupId);
                var alarmPoints = await _alarmPointRepository.GetAlarmPointsByGroupIdAsync(groupId);
                _logger.LogInformation("그룹별 알람 포인트 조회 완료: 그룹ID={GroupId}, {Count}개 조회됨", groupId, alarmPoints.Count());
                return alarmPoints;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "그룹별 알람 포인트 조회 중 오류 발생: 그룹ID={GroupId}", groupId);
                return new List<AlarmPoint>();
            }
        }

        // 알람 포인트 추가
        public async Task<bool> AddAlarmPointAsync(AlarmPoint alarmPoint)
        {
            if (alarmPoint == null)
            {
                _logger.LogError("알람 포인트 추가 실패: 알람 포인트가 null입니다.");
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
                _logger.LogError(ex, "알람 포인트 추가 중 오류 발생: 이름={Name}", alarmPoint.Name);
                return false;
            }
        }

        // 알람 포인트 업데이트
        public async Task<bool> UpdateAlarmPointAsync(AlarmPoint alarmPoint)
        {
            if (alarmPoint == null)
            {
                _logger.LogError("알람 포인트 업데이트 실패: 알람 포인트가 null입니다.");
                return false;
            }

            try
            {
                _logger.LogInformation("알람 포인트 업데이트 시작: ID={Id}, 이름={Name}", alarmPoint.Id, alarmPoint.Name);
                await _alarmPointRepository.UpdateAsync(alarmPoint);
                _logger.LogInformation("알람 포인트 업데이트 완료: ID={Id}, 이름={Name}", alarmPoint.Id, alarmPoint.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "알람 포인트 업데이트 중 오류 발생: ID={Id}, 이름={Name}", alarmPoint.Id, alarmPoint.Name);
                return false;
            }
        }

        // 알람 포인트 삭제
        public async Task<bool> DeleteAlarmPointAsync(int id)
        {
            try
            {
                _logger.LogInformation("알람 포인트 삭제 시작: ID={Id}", id);
                var alarmPoint = await _alarmPointRepository.GetByIdAsync(id);

                if (alarmPoint == null)
                {
                    _logger.LogWarning("알람 포인트 삭제 실패: 알람 포인트를 찾을 수 없음, ID={Id}", id);
                    return false;
                }

                await _alarmPointRepository.DeleteAsync(alarmPoint);
                _logger.LogInformation("알람 포인트 삭제 완료: ID={Id}, 이름={Name}", id, alarmPoint.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "알람 포인트 삭제 중 오류 발생: ID={Id}", id);
                return false;
            }
        }

        // 알람 발생 시 메시지 발송
        public async Task<bool> TriggerAlarmAsync(int alarmPointId, string messageContent)
        {
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

                if (!messageGroup.IsActive)
                {
                    _logger.LogWarning("알람 트리거 실패: 메시지 그룹이 비활성 상태임, ID={MessageGroupId}, 이름={Name}", messageGroup.Id, messageGroup.Name);
                    return false;
                }

                // 그룹에 속한 회원들 가져오기
                var members = await _memberRepository.GetMembersByGroupIdAsync(messageGroup.Id);
                var activeMembers = members.Where(m => m.IsActive).ToList();

                if (!activeMembers.Any())
                {
                    _logger.LogWarning("알람 트리거 실패: 활성 상태인 회원이 없음, 그룹ID={MessageGroupId}, 그룹명={GroupName}", messageGroup.Id, messageGroup.Name);
                    return false;
                }

                // 메시지 발송 기록 저장
                var message = new Message
                {
                    Content = messageContent,
                    SendTime = DateTime.Now,
                    AlarmPointId = alarmPointId,
                    MessageGroupId = messageGroup.Id,
                    Status = "발송 중"
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
                await _messageRepository.UpdateAsync(message);

                _logger.LogInformation("알람 트리거 완료: 알람포인트ID={AlarmPointId}, 성공={SuccessCount}/{TotalCount}",
                    alarmPointId, successCount, totalCount);

                return successCount > 0; // 하나 이상 성공했으면 true 반환
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "알람 트리거 중 오류 발생: 알람포인트ID={AlarmPointId}", alarmPointId);
                return false;
            }
        }
    }
}
