// 작성자: Sangchan, Kim
// 작성일: 2025-03-31
// 기능: 회원 관련 비즈니스 로직을 처리하는 서비스
// 설명: 회원 CRUD 및 관련 기능을 제공합니다.

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
    /// 회원 관련 비즈니스 로직을 처리하는 서비스 클래스
    /// 회원 조회, 추가, 수정, 삭제 등의 기능을 제공합니다.
    /// </summary>
    public class MemberService : IMemberService
    {
        // 회원 데이터 접근을 위한 리포지토리
        private readonly IMemberRepository _memberRepository;

        // 로깅을 위한 로거 인스턴스
        private readonly ILogger<MemberService> _logger;

        /// <summary>
        /// MemberService 생성자
        /// </summary>
        /// <param name="memberRepository">회원 리포지토리 인스턴스</param>
        /// <param name="logger">로깅을 위한 ILogger 인스턴스</param>
        /// <exception cref="ArgumentNullException">필수 매개변수가 null인 경우 발생</exception>
        public MemberService(IMemberRepository memberRepository, ILogger<MemberService> logger)
        {
            // null 체크 및 예외 처리
            _memberRepository = memberRepository ?? throw new ArgumentNullException(nameof(memberRepository), "회원 리포지토리는 null이 될 수 없습니다.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "로거는 null이 될 수 없습니다.");

            _logger.LogInformation("MemberService 초기화 완료");
        }

        /// <summary>
        /// 모든 회원을 조회합니다.
        /// </summary>
        /// <returns>회원 목록 또는 오류 발생 시 빈 목록</returns>
        public async Task<IEnumerable<Member>> GetAllMembersAsync()
        {
            try
            {
                _logger.LogInformation("모든 회원 조회 시작");
                var members = await _memberRepository.GetAllAsync();

                // null 체크 (방어적 프로그래밍)
                if (members == null)
                {
                    _logger.LogWarning("리포지토리에서 null 반환됨. 빈 목록으로 대체합니다.");
                    return new List<Member>();
                }

                _logger.LogInformation("모든 회원 조회 완료: {Count}명 조회됨", members.Count());
                return members;
            }
            catch (Exception ex)
            {
                // 예외 발생 시 로깅 후 빈 목록 반환
                _logger.LogError(ex, "모든 회원 조회 중 오류 발생: {ErrorMessage}", ex.Message);
                return new List<Member>();
            }
        }

        /// <summary>
        /// ID로 특정 회원을 조회합니다.
        /// </summary>
        /// <param name="id">조회할 회원 ID</param>
        /// <returns>조회된 회원 또는 null(회원이 없거나 오류 발생 시)</returns>
        public async Task<Member?> GetMemberByIdAsync(int id)
        {
            // ID 유효성 검사
            if (id <= 0)
            {
                _logger.LogWarning("회원 조회 실패: 유효하지 않은 ID={Id}", id);
                return null;
            }

            try
            {
                _logger.LogInformation("회원 조회 시작: ID={Id}", id);
                var member = await _memberRepository.GetByIdAsync(id);

                if (member == null)
                {
                    _logger.LogWarning("회원을 찾을 수 없음: ID={Id}", id);
                }
                else
                {
                    _logger.LogInformation("회원 조회 성공: ID={Id}, 이름={Name}", id, member.Name);
                }

                return member;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "회원 조회 중 오류 발생: ID={Id}, 오류={ErrorMessage}", id, ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 전화번호로 회원을 조회합니다.
        /// </summary>
        /// <param name="phoneNumber">조회할 회원의 전화번호</param>
        /// <returns>조회된 회원 또는 null(회원이 없거나 오류 발생 시)</returns>
        public async Task<Member?> GetMemberByPhoneNumberAsync(string phoneNumber)
        {
            // 전화번호 유효성 검사
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                _logger.LogWarning("전화번호로 회원 조회 실패: 유효하지 않은 전화번호");
                return null;
            }

            try
            {
                _logger.LogInformation("전화번호로 회원 조회 시작: PhoneNumber={PhoneNumber}", phoneNumber);
                var member = await _memberRepository.GetByPhoneNumberAsync(phoneNumber);

                if (member == null)
                {
                    _logger.LogWarning("전화번호로 회원을 찾을 수 없음: PhoneNumber={PhoneNumber}", phoneNumber);
                }
                else
                {
                    _logger.LogInformation("전화번호로 회원 조회 성공: PhoneNumber={PhoneNumber}, 이름={Name}", phoneNumber, member.Name);
                }

                return member;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "전화번호로 회원 조회 중 오류 발생: PhoneNumber={PhoneNumber}, 오류={ErrorMessage}",
                    phoneNumber, ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 새 회원을 추가합니다.
        /// </summary>
        /// <param name="member">추가할 회원 객체</param>
        /// <returns>추가 성공 여부</returns>
        public async Task<bool> AddMemberAsync(Member member)
        {
            // null 체크
            if (member == null)
            {
                _logger.LogError("회원 추가 실패: 회원 객체가 null입니다.");
                return false;
            }

            // 필수 필드 유효성 검사
            if (string.IsNullOrWhiteSpace(member.Name))
            {
                _logger.LogError("회원 추가 실패: 이름이 비어 있습니다.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(member.PhoneNumber))
            {
                _logger.LogError("회원 추가 실패: 전화번호가 비어 있습니다.");
                return false;
            }

            try
            {
                _logger.LogInformation("회원 추가 시작: 이름={Name}, 전화번호={PhoneNumber}", member.Name, member.PhoneNumber);

                // 동일한 전화번호를 가진 회원이 이미 존재하는지 확인 (선택적)
                var existingMember = await _memberRepository.GetByPhoneNumberAsync(member.PhoneNumber);
                if (existingMember != null)
                {
                    _logger.LogWarning("회원 추가 실패: 동일한 전화번호를 가진 회원이 이미 존재합니다. 전화번호={PhoneNumber}",
                        member.PhoneNumber);
                    return false;
                }

                await _memberRepository.AddAsync(member);
                _logger.LogInformation("회원 추가 완료: ID={Id}, 이름={Name}", member.Id, member.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "회원 추가 중 오류 발생: 이름={Name}, 오류={ErrorMessage}",
                    member.Name, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 기존 회원 정보를 업데이트합니다.
        /// </summary>
        /// <param name="member">업데이트할 회원 객체</param>
        /// <returns>업데이트 성공 여부</returns>
        public async Task<bool> UpdateMemberAsync(Member member)
        {
            // null 체크
            if (member == null)
            {
                _logger.LogError("회원 업데이트 실패: 회원 객체가 null입니다.");
                return false;
            }

            // ID 유효성 검사
            if (member.Id <= 0)
            {
                _logger.LogError("회원 업데이트 실패: 유효하지 않은 ID={Id}", member.Id);
                return false;
            }

            // 필수 필드 유효성 검사
            if (string.IsNullOrWhiteSpace(member.Name))
            {
                _logger.LogError("회원 업데이트 실패: 이름이 비어 있습니다. ID={Id}", member.Id);
                return false;
            }

            if (string.IsNullOrWhiteSpace(member.PhoneNumber))
            {
                _logger.LogError("회원 업데이트 실패: 전화번호가 비어 있습니다. ID={Id}", member.Id);
                return false;
            }

            try
            {
                // 회원 존재 여부 확인
                var existingMember = await _memberRepository.GetByIdAsync(member.Id);
                if (existingMember == null)
                {
                    _logger.LogWarning("회원 업데이트 실패: 회원을 찾을 수 없음, ID={Id}", member.Id);
                    return false;
                }

                _logger.LogInformation("회원 업데이트 시작: ID={Id}, 이름={Name}", member.Id, member.Name);
                await _memberRepository.UpdateAsync(member);
                _logger.LogInformation("회원 업데이트 완료: ID={Id}, 이름={Name}", member.Id, member.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "회원 업데이트 중 오류 발생: ID={Id}, 이름={Name}, 오류={ErrorMessage}",
                    member.Id, member.Name, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 회원을 삭제합니다.
        /// </summary>
        /// <param name="id">삭제할 회원 ID</param>
        /// <returns>삭제 성공 여부</returns>
        public async Task<bool> DeleteMemberAsync(int id)
        {
            // ID 유효성 검사
            if (id <= 0)
            {
                _logger.LogError("회원 삭제 실패: 유효하지 않은 ID={Id}", id);
                return false;
            }

            try
            {
                _logger.LogInformation("회원 삭제 시작: ID={Id}", id);

                // 회원 존재 여부 확인
                var member = await _memberRepository.GetByIdAsync(id);
                if (member == null)
                {
                    _logger.LogWarning("회원 삭제 실패: 회원을 찾을 수 없음, ID={Id}", id);
                    return false;
                }

                await _memberRepository.DeleteAsync(member);
                _logger.LogInformation("회원 삭제 완료: ID={Id}, 이름={Name}", id, member.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "회원 삭제 중 오류 발생: ID={Id}, 오류={ErrorMessage}", id, ex.Message);
                return false;
            }
        }
    }
}
