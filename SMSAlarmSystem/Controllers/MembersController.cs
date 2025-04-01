// 작성자: Sangchan, Kim
// 작성일: 2025-03-31
// 기능: 회원 관리 컨트롤러
// 설명: 회원 목록 조회, 상세 보기, 추가, 수정, 삭제 기능을 제공하는 MVC 컨트롤러입니다.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMSAlarmSystem.Core.Models;
using SMSAlarmSystem.Models;
using SMSAlarmSystem.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace SMSAlarmSystem.Controllers
{
    /// <summary>
    /// 회원 관리 기능을 제공하는 MVC 컨트롤러
    /// 회원 목록 조회, 상세 보기, 추가, 수정, 삭제 기능을 제공합니다.
    /// </summary>
    public class MembersController : Controller
    {
        // 회원 관련 비즈니스 로직을 처리하는 서비스
        private readonly IMemberService _memberService;

        // 로깅을 위한 로거 인스턴스
        private readonly ILogger<MembersController> _logger;

        /// <summary>
        /// MembersController 생성자
        /// </summary>
        /// <param name="memberService">회원 서비스 인스턴스</param>
        /// <param name="logger">로깅을 위한 ILogger 인스턴스</param>
        /// <exception cref="ArgumentNullException">필수 매개변수가 null인 경우 발생</exception>
        public MembersController(IMemberService memberService, ILogger<MembersController> logger)
        {
            _memberService = memberService ?? throw new ArgumentNullException(nameof(memberService), "회원 서비스는 null이 될 수 없습니다.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "로거는 null이 될 수 없습니다.");

            _logger.LogInformation("MembersController 초기화 완료");
        }

        /// <summary>
        /// 회원 목록 페이지를 표시합니다.
        /// </summary>
        /// <param name="page">페이지 번호 (1부터 시작)</param>
        /// <param name="pageSize">페이지당 표시할 회원 수</param>
        /// <returns>페이지네이션이 적용된 회원 목록 뷰</returns>
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            try
            {
                // 페이지 번호와 페이지 크기 유효성 검사
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 10;

                _logger.LogInformation("회원 목록 조회 시작: 페이지={Page}, 페이지크기={PageSize}", page, pageSize);

                // 모든 회원 조회
                var allMembers = await _memberService.GetAllMembersAsync();

                // null 체크 (방어적 프로그래밍)
                if (allMembers == null)
                {
                    _logger.LogWarning("회원 서비스에서 null 반환됨. 빈 목록으로 대체합니다.");
                    allMembers = new List<Member>();
                }

                // 페이지네이션 적용
                var totalItems = allMembers.Count();
                var pagedMembers = allMembers
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // 페이지네이션 뷰 모델 생성
                var model = new PaginatedViewModel<Member>
                {
                    Items = pagedMembers,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalItems
                };

                _logger.LogInformation("회원 목록 조회 완료: 총 {TotalItems}명 중 {PagedCount}명 표시",
                    totalItems, pagedMembers.Count);

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "회원 목록 조회 중 오류 발생: {ErrorMessage}", ex.Message);

                // 오류 발생 시 빈 모델로 뷰 표시
                TempData["ErrorMessage"] = "회원 목록을 불러오는 중 오류가 발생했습니다.";
                return View(new PaginatedViewModel<Member>());
            }
        }

        /// <summary>
        /// 회원 상세 정보 페이지를 표시합니다.
        /// </summary>
        /// <param name="id">조회할 회원 ID</param>
        /// <returns>회원 상세 뷰 또는 NotFound</returns>
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("잘못된 회원 ID: {Id}", id);
                    return BadRequest("유효하지 않은 회원 ID입니다.");
                }

                _logger.LogInformation("회원 상세 조회 시작: ID={Id}", id);
                var member = await _memberService.GetMemberByIdAsync(id);

                if (member == null)
                {
                    _logger.LogWarning("회원을 찾을 수 없음: ID={Id}", id);
                    return NotFound($"ID가 {id}인 회원을 찾을 수 없습니다.");
                }

                _logger.LogInformation("회원 상세 조회 완료: ID={Id}, 이름={Name}", id, member.Name);
                return View(member);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "회원 상세 조회 중 오류 발생: ID={Id}, 오류={ErrorMessage}", id, ex.Message);

                // 오류 메시지를 TempData에 저장하여 뷰에서 표시
                TempData["ErrorMessage"] = $"회원 정보를 불러오는 중 오류가 발생했습니다: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 회원 생성 페이지를 표시합니다.
        /// </summary>
        /// <returns>회원 생성 뷰</returns>
        public IActionResult Create()
        {
            _logger.LogInformation("회원 생성 페이지 요청");
            return View();
        }

        /// <summary>
        /// 회원 생성을 처리합니다.
        /// </summary>
        /// <param name="member">생성할 회원 정보</param>
        /// <returns>성공 시 회원 목록 페이지로 리디렉션, 실패 시 생성 페이지 다시 표시</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Member member)
        {
            try
            {
                // null 체크
                if (member == null)
                {
                    _logger.LogWarning("회원 생성 실패: 회원 객체가 null입니다.");
                    ModelState.AddModelError(string.Empty, "회원 정보가 제공되지 않았습니다.");
                    return View();
                }

                if (ModelState.IsValid)
                {
                    _logger.LogInformation("회원 생성 시작: 이름={Name}, 전화번호={PhoneNumber}", member.Name, member.PhoneNumber);

                    var result = await _memberService.AddMemberAsync(member);
                    if (result)
                    {
                        _logger.LogInformation("회원 생성 성공: ID={Id}, 이름={Name}", member.Id, member.Name);
                        TempData["SuccessMessage"] = "회원이 성공적으로 생성되었습니다.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        _logger.LogWarning("회원 생성 실패: 이름={Name}, 전화번호={PhoneNumber}", member.Name, member.PhoneNumber);
                        ModelState.AddModelError(string.Empty, "회원 생성에 실패했습니다. 다시 시도해 주세요.");
                    }
                }
                else
                {
                    _logger.LogWarning("회원 생성 유효성 검사 실패");
                }

                return View(member);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "회원 생성 중 오류 발생: 이름={Name}, 오류={ErrorMessage}",
                    member?.Name ?? "Unknown", ex.Message);

                ModelState.AddModelError(string.Empty, $"회원 생성 중 오류가 발생했습니다: {ex.Message}");
                return View(member);
            }
        }

        /// <summary>
        /// 회원 수정 페이지를 표시합니다.
        /// </summary>
        /// <param name="id">수정할 회원 ID</param>
        /// <returns>회원 수정 뷰 또는 NotFound</returns>
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("잘못된 회원 ID: {Id}", id);
                    return BadRequest("유효하지 않은 회원 ID입니다.");
                }

                _logger.LogInformation("회원 수정 페이지 요청: ID={Id}", id);
                var member = await _memberService.GetMemberByIdAsync(id);

                if (member == null)
                {
                    _logger.LogWarning("회원을 찾을 수 없음: ID={Id}", id);
                    return NotFound($"ID가 {id}인 회원을 찾을 수 없습니다.");
                }

                return View(member);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "회원 수정 페이지 로드 중 오류 발생: ID={Id}, 오류={ErrorMessage}", id, ex.Message);

                TempData["ErrorMessage"] = $"회원 정보를 불러오는 중 오류가 발생했습니다: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 회원 수정을 처리합니다.
        /// </summary>
        /// <param name="id">수정할 회원 ID</param>
        /// <param name="member">수정된 회원 정보</param>
        /// <returns>성공 시 회원 목록 페이지로 리디렉션, 실패 시 수정 페이지 다시 표시</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Member member)
        {
            try
            {
                // null 체크
                if (member == null)
                {
                    _logger.LogWarning("회원 수정 실패: 회원 객체가 null입니다.");
                    ModelState.AddModelError(string.Empty, "회원 정보가 제공되지 않았습니다.");
                    return View();
                }

                // ID 일치 여부 확인
                if (id != member.Id)
                {
                    _logger.LogWarning("회원 수정 실패: URL의 ID({UrlId})와 폼의 ID({FormId})가 일치하지 않습니다.", id, member.Id);
                    return NotFound($"ID가 일치하지 않습니다.");
                }

                if (ModelState.IsValid)
                {
                    _logger.LogInformation("회원 수정 시작: ID={Id}, 이름={Name}", id, member.Name);

                    var result = await _memberService.UpdateMemberAsync(member);
                    if (result)
                    {
                        _logger.LogInformation("회원 수정 성공: ID={Id}, 이름={Name}", id, member.Name);
                        TempData["SuccessMessage"] = "회원 정보가 성공적으로 수정되었습니다.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        _logger.LogWarning("회원 수정 실패: ID={Id}, 이름={Name}", id, member.Name);
                        ModelState.AddModelError(string.Empty, "회원 정보 수정에 실패했습니다. 다시 시도해 주세요.");
                    }
                }
                else
                {
                    _logger.LogWarning("회원 수정 유효성 검사 실패: ID={Id}", id);
                }

                return View(member);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "회원 수정 중 오류 발생: ID={Id}, 오류={ErrorMessage}", id, ex.Message);

                ModelState.AddModelError(string.Empty, $"회원 정보 수정 중 오류가 발생했습니다: {ex.Message}");
                return View(member);
            }
        }

        /// <summary>
        /// 회원 삭제 확인 페이지를 표시합니다.
        /// </summary>
        /// <param name="id">삭제할 회원 ID</param>
        /// <returns>회원 삭제 확인 뷰 또는 NotFound</returns>
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("잘못된 회원 ID: {Id}", id);
                    return BadRequest("유효하지 않은 회원 ID입니다.");
                }

                _logger.LogInformation("회원 삭제 페이지 요청: ID={Id}", id);
                var member = await _memberService.GetMemberByIdAsync(id);

                if (member == null)
                {
                    _logger.LogWarning("회원을 찾을 수 없음: ID={Id}", id);
                    return NotFound($"ID가 {id}인 회원을 찾을 수 없습니다.");
                }

                return View(member);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "회원 삭제 페이지 로드 중 오류 발생: ID={Id}, 오류={ErrorMessage}", id, ex.Message);

                TempData["ErrorMessage"] = $"회원 정보를 불러오는 중 오류가 발생했습니다: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 회원 삭제를 처리합니다.
        /// </summary>
        /// <param name="id">삭제할 회원 ID</param>
        /// <returns>회원 목록 페이지로 리디렉션</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("잘못된 회원 ID: {Id}", id);
                    TempData["ErrorMessage"] = "유효하지 않은 회원 ID입니다.";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation("회원 삭제 시작: ID={Id}", id);

                var result = await _memberService.DeleteMemberAsync(id);
                if (result)
                {
                    _logger.LogInformation("회원 삭제 성공: ID={Id}", id);
                    TempData["SuccessMessage"] = "회원이 성공적으로 삭제되었습니다.";
                }
                else
                {
                    _logger.LogWarning("회원 삭제 실패: ID={Id}", id);
                    TempData["ErrorMessage"] = "회원 삭제에 실패했습니다. 다시 시도해 주세요.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "회원 삭제 중 오류 발생: ID={Id}, 오류={ErrorMessage}", id, ex.Message);

                TempData["ErrorMessage"] = $"회원 삭제 중 오류가 발생했습니다: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
