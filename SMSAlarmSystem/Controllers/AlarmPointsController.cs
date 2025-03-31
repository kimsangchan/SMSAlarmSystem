// 작성자: Sangchan, Kim
// 작성일: 2025-03-31
// 기능: 알람 포인트 관리 컨트롤러
// 설명: 알람 포인트 목록 조회, 상세 보기, 추가, 수정, 삭제 및 알람 트리거 기능을 제공하는 MVC 컨트롤러입니다.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using SMSAlarmSystem.Core.Models;
using SMSAlarmSystem.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace SMSAlarmSystem.Controllers
{
    /// <summary>
    /// 알람 포인트 관리 기능을 제공하는 MVC 컨트롤러
    /// 알람 포인트 목록 조회, 상세 보기, 추가, 수정, 삭제 및 알람 트리거 기능을 제공합니다.
    /// </summary>
    public class AlarmPointsController : Controller
    {
        // 알람 관련 비즈니스 로직을 처리하는 서비스
        private readonly IAlarmService _alarmService;

        // 메시지 그룹 관련 비즈니스 로직을 처리하는 서비스
        private readonly IMessageGroupService _messageGroupService;

        // 로깅을 위한 로거 인스턴스
        private readonly ILogger<AlarmPointsController> _logger;

        /// <summary>
        /// AlarmPointsController 생성자
        /// </summary>
        /// <param name="alarmService">알람 서비스 인스턴스</param>
        /// <param name="messageGroupService">메시지 그룹 서비스 인스턴스</param>
        /// <param name="logger">로깅을 위한 ILogger 인스턴스</param>
        /// <exception cref="ArgumentNullException">필수 매개변수가 null인 경우 발생</exception>
        public AlarmPointsController(
            IAlarmService alarmService,
            IMessageGroupService messageGroupService,
            ILogger<AlarmPointsController> logger)
        {
            _alarmService = alarmService ?? throw new ArgumentNullException(nameof(alarmService), "알람 서비스는 null이 될 수 없습니다.");
            _messageGroupService = messageGroupService ?? throw new ArgumentNullException(nameof(messageGroupService), "메시지 그룹 서비스는 null이 될 수 없습니다.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "로거는 null이 될 수 없습니다.");

            _logger.LogInformation("AlarmPointsController 초기화 완료");
        }

        /// <summary>
        /// 알람 포인트 목록 페이지를 표시합니다.
        /// </summary>
        /// <returns>알람 포인트 목록 뷰</returns>
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("알람 포인트 목록 조회 시작");
                var alarmPoints = await _alarmService.GetAllAlarmPointsAsync();
                _logger.LogInformation("알람 포인트 목록 조회 완료");
                return View(alarmPoints);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "알람 포인트 목록 조회 중 오류 발생: {ErrorMessage}", ex.Message);

                // 오류 메시지를 TempData에 저장하여 뷰에서 표시
                TempData["ErrorMessage"] = "알람 포인트 목록을 불러오는 중 오류가 발생했습니다.";
                return View(Array.Empty<AlarmPoint>());
            }
        }

        /// <summary>
        /// 알람 포인트 상세 정보 페이지를 표시합니다.
        /// </summary>
        /// <param name="id">조회할 알람 포인트 ID</param>
        /// <returns>알람 포인트 상세 뷰 또는 NotFound</returns>
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("잘못된 알람 포인트 ID: {Id}", id);
                    return BadRequest("유효하지 않은 알람 포인트 ID입니다.");
                }

                _logger.LogInformation("알람 포인트 상세 조회 시작: ID={Id}", id);
                var alarmPoint = await _alarmService.GetAlarmPointByIdAsync(id);

                if (alarmPoint == null)
                {
                    _logger.LogWarning("알람 포인트를 찾을 수 없음: ID={Id}", id);
                    return NotFound($"ID가 {id}인 알람 포인트를 찾을 수 없습니다.");
                }

                _logger.LogInformation("알람 포인트 상세 조회 완료: ID={Id}, 이름={Name}", id, alarmPoint.Name);
                return View(alarmPoint);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "알람 포인트 상세 조회 중 오류 발생: ID={Id}, 오류={ErrorMessage}", id, ex.Message);

                // 오류 메시지를 TempData에 저장하여 뷰에서 표시
                TempData["ErrorMessage"] = $"알람 포인트 정보를 불러오는 중 오류가 발생했습니다: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 알람 포인트 생성 페이지를 표시합니다.
        /// </summary>
        /// <returns>알람 포인트 생성 뷰</returns>
        public async Task<IActionResult> Create()
        {
            try
            {
                _logger.LogInformation("알람 포인트 생성 페이지 요청");

                var groups = await _messageGroupService.GetActiveGroupsAsync();
                ViewBag.MessageGroupId = new SelectList(groups, "Id", "Name");

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "알람 포인트 생성 페이지 로드 중 오류 발생: {ErrorMessage}", ex.Message);

                TempData["ErrorMessage"] = $"메시지 그룹 목록을 불러오는 중 오류가 발생했습니다: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 알람 포인트 생성을 처리합니다.
        /// </summary>
        /// <param name="alarmPoint">생성할 알람 포인트 정보</param>
        /// <returns>성공 시 알람 포인트 목록 페이지로 리디렉션, 실패 시 생성 페이지 다시 표시</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AlarmPoint alarmPoint)
        {
            try
            {
                // null 체크
                if (alarmPoint == null)
                {
                    _logger.LogWarning("알람 포인트 생성 실패: 알람 포인트 객체가 null입니다.");
                    ModelState.AddModelError(string.Empty, "알람 포인트 정보가 제공되지 않았습니다.");
                    return View();
                }

                if (ModelState.IsValid)
                {
                    _logger.LogInformation("알람 포인트 생성 시작: 이름={Name}", alarmPoint.Name);

                    var result = await _alarmService.AddAlarmPointAsync(alarmPoint);
                    if (result)
                    {
                        _logger.LogInformation("알람 포인트 생성 성공: ID={Id}, 이름={Name}", alarmPoint.Id, alarmPoint.Name);
                        TempData["SuccessMessage"] = "알람 포인트가 성공적으로 생성되었습니다.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        _logger.LogWarning("알람 포인트 생성 실패: 이름={Name}", alarmPoint.Name);
                        ModelState.AddModelError(string.Empty, "알람 포인트 생성에 실패했습니다. 다시 시도해 주세요.");
                    }
                }
                else
                {
                    _logger.LogWarning("알람 포인트 생성 유효성 검사 실패");
                }

                // 실패 시 드롭다운 목록 다시 로드
                var groups = await _messageGroupService.GetActiveGroupsAsync();
                ViewBag.MessageGroupId = new SelectList(groups, "Id", "Name", alarmPoint.MessageGroupId);

                return View(alarmPoint);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "알람 포인트 생성 중 오류 발생: 이름={Name}, 오류={ErrorMessage}",
                    alarmPoint?.Name ?? "Unknown", ex.Message);

                ModelState.AddModelError(string.Empty, $"알람 포인트 생성 중 오류가 발생했습니다: {ex.Message}");

                try
                {
                    var groups = await _messageGroupService.GetActiveGroupsAsync();
                    ViewBag.MessageGroupId = new SelectList(groups, "Id", "Name", alarmPoint?.MessageGroupId);
                }
                catch
                {
                    // 그룹 목록 로드 실패 시 빈 목록 사용
                    ViewBag.MessageGroupId = new SelectList(Array.Empty<MessageGroup>(), "Id", "Name");
                }

                return View(alarmPoint);
            }
        }

        /// <summary>
        /// 알람 포인트 수정 페이지를 표시합니다.
        /// </summary>
        /// <param name="id">수정할 알람 포인트 ID</param>
        /// <returns>알람 포인트 수정 뷰 또는 NotFound</returns>
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("잘못된 알람 포인트 ID: {Id}", id);
                    return BadRequest("유효하지 않은 알람 포인트 ID입니다.");
                }

                _logger.LogInformation("알람 포인트 수정 페이지 요청: ID={Id}", id);
                var alarmPoint = await _alarmService.GetAlarmPointByIdAsync(id);

                if (alarmPoint == null)
                {
                    _logger.LogWarning("알람 포인트를 찾을 수 없음: ID={Id}", id);
                    return NotFound($"ID가 {id}인 알람 포인트를 찾을 수 없습니다.");
                }

                var groups = await _messageGroupService.GetActiveGroupsAsync();
                ViewBag.MessageGroupId = new SelectList(groups, "Id", "Name", alarmPoint.MessageGroupId);

                return View(alarmPoint);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "알람 포인트 수정 페이지 로드 중 오류 발생: ID={Id}, 오류={ErrorMessage}", id, ex.Message);

                TempData["ErrorMessage"] = $"알람 포인트 정보를 불러오는 중 오류가 발생했습니다: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 알람 포인트 수정을 처리합니다.
        /// </summary>
        /// <param name="id">수정할 알람 포인트 ID</param>
        /// <param name="alarmPoint">수정된 알람 포인트 정보</param>
        /// <returns>성공 시 알람 포인트 목록 페이지로 리디렉션, 실패 시 수정 페이지 다시 표시</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AlarmPoint alarmPoint)
        {
            try
            {
                // null 체크
                if (alarmPoint == null)
                {
                    _logger.LogWarning("알람 포인트 수정 실패: 알람 포인트 객체가 null입니다.");
                    ModelState.AddModelError(string.Empty, "알람 포인트 정보가 제공되지 않았습니다.");
                    return View();
                }

                // ID 일치 여부 확인
                if (id != alarmPoint.Id)
                {
                    _logger.LogWarning("알람 포인트 수정 실패: URL의 ID({UrlId})와 폼의 ID({FormId})가 일치하지 않습니다.", id, alarmPoint.Id);
                    return NotFound($"ID가 일치하지 않습니다.");
                }

                if (ModelState.IsValid)
                {
                    _logger.LogInformation("알람 포인트 수정 시작: ID={Id}, 이름={Name}", id, alarmPoint.Name);

                    var result = await _alarmService.UpdateAlarmPointAsync(alarmPoint);
                    if (result)
                    {
                        _logger.LogInformation("알람 포인트 수정 성공: ID={Id}, 이름={Name}", id, alarmPoint.Name);
                        TempData["SuccessMessage"] = "알람 포인트 정보가 성공적으로 수정되었습니다.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        _logger.LogWarning("알람 포인트 수정 실패: ID={Id}, 이름={Name}", id, alarmPoint.Name);
                        ModelState.AddModelError(string.Empty, "알람 포인트 정보 수정에 실패했습니다. 다시 시도해 주세요.");
                    }
                }
                else
                {
                    _logger.LogWarning("알람 포인트 수정 유효성 검사 실패: ID={Id}", id);
                }

                // 실패 시 드롭다운 목록 다시 로드
                var groups = await _messageGroupService.GetActiveGroupsAsync();
                ViewBag.MessageGroupId = new SelectList(groups, "Id", "Name", alarmPoint.MessageGroupId);

                return View(alarmPoint);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "알람 포인트 수정 중 오류 발생: ID={Id}, 오류={ErrorMessage}", id, ex.Message);

                ModelState.AddModelError(string.Empty, $"알람 포인트 정보 수정 중 오류가 발생했습니다: {ex.Message}");

                try
                {
                    var groups = await _messageGroupService.GetActiveGroupsAsync();
                    ViewBag.MessageGroupId = new SelectList(groups, "Id", "Name", alarmPoint.MessageGroupId);
                }
                catch
                {
                    // 그룹 목록 로드 실패 시 빈 목록 사용
                    ViewBag.MessageGroupId = new SelectList(Array.Empty<MessageGroup>(), "Id", "Name");
                }

                return View(alarmPoint);
            }
        }

        /// <summary>
        /// 알람 포인트 삭제 확인 페이지를 표시합니다.
        /// </summary>
        /// <param name="id">삭제할 알람 포인트 ID</param>
        /// <returns>알람 포인트 삭제 확인 뷰 또는 NotFound</returns>
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("잘못된 알람 포인트 ID: {Id}", id);
                    return BadRequest("유효하지 않은 알람 포인트 ID입니다.");
                }

                _logger.LogInformation("알람 포인트 삭제 페이지 요청: ID={Id}", id);
                var alarmPoint = await _alarmService.GetAlarmPointByIdAsync(id);

                if (alarmPoint == null)
                {
                    _logger.LogWarning("알람 포인트를 찾을 수 없음: ID={Id}", id);
                    return NotFound($"ID가 {id}인 알람 포인트를 찾을 수 없습니다.");
                }

                return View(alarmPoint);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "알람 포인트 삭제 페이지 로드 중 오류 발생: ID={Id}, 오류={ErrorMessage}", id, ex.Message);

                TempData["ErrorMessage"] = $"알람 포인트 정보를 불러오는 중 오류가 발생했습니다: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 알람 포인트 삭제를 처리합니다.
        /// </summary>
        /// <param name="id">삭제할 알람 포인트 ID</param>
        /// <returns>알람 포인트 목록 페이지로 리디렉션</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("잘못된 알람 포인트 ID: {Id}", id);
                    TempData["ErrorMessage"] = "유효하지 않은 알람 포인트 ID입니다.";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation("알람 포인트 삭제 시작: ID={Id}", id);

                var result = await _alarmService.DeleteAlarmPointAsync(id);
                if (result)
                {
                    _logger.LogInformation("알람 포인트 삭제 성공: ID={Id}", id);
                    TempData["SuccessMessage"] = "알람 포인트가 성공적으로 삭제되었습니다.";
                }
                else
                {
                    _logger.LogWarning("알람 포인트 삭제 실패: ID={Id}", id);
                    TempData["ErrorMessage"] = "알람 포인트 삭제에 실패했습니다. 다시 시도해 주세요.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "알람 포인트 삭제 중 오류 발생: ID={Id}, 오류={ErrorMessage}", id, ex.Message);

                TempData["ErrorMessage"] = $"알람 포인트 삭제 중 오류가 발생했습니다: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 알람 트리거 페이지를 표시합니다.
        /// </summary>
        /// <param name="id">트리거할 알람 포인트 ID</param>
        /// <returns>알람 트리거 뷰 또는 NotFound</returns>
        public async Task<IActionResult> Trigger(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("잘못된 알람 포인트 ID: {Id}", id);
                    return BadRequest("유효하지 않은 알람 포인트 ID입니다.");
                }

                _logger.LogInformation("알람 트리거 페이지 요청: ID={Id}", id);
                var alarmPoint = await _alarmService.GetAlarmPointByIdAsync(id);

                if (alarmPoint == null)
                {
                    _logger.LogWarning("알람 포인트를 찾을 수 없음: ID={Id}", id);
                    return NotFound($"ID가 {id}인 알람 포인트를 찾을 수 없습니다.");
                }

                return View(alarmPoint);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "알람 트리거 페이지 로드 중 오류 발생: ID={Id}, 오류={ErrorMessage}", id, ex.Message);

                TempData["ErrorMessage"] = $"알람 포인트 정보를 불러오는 중 오류가 발생했습니다: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 알람 트리거를 처리합니다.
        /// </summary>
        /// <param name="id">트리거할 알람 포인트 ID</param>
        /// <param name="messageContent">발송할 메시지 내용</param>
        /// <returns>알람 포인트 목록 페이지로 리디렉션</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Trigger(int id, string messageContent)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("잘못된 알람 포인트 ID: {Id}", id);
                    TempData["ErrorMessage"] = "유효하지 않은 알람 포인트 ID입니다.";
                    return RedirectToAction(nameof(Index));
                }

                if (string.IsNullOrWhiteSpace(messageContent))
                {
                    _logger.LogWarning("알람 트리거 실패: 메시지 내용이 비어 있습니다. ID={Id}", id);

                    ModelState.AddModelError("messageContent", "메시지 내용은 필수입니다.");
                    var alarmPoint = await _alarmService.GetAlarmPointByIdAsync(id);
                    return View(alarmPoint);
                }

                _logger.LogInformation("알람 트리거 시작: ID={Id}, 메시지={Message}", id, messageContent);

                var result = await _alarmService.TriggerAlarmAsync(id, messageContent);
                if (result)
                {
                    _logger.LogInformation("알람 트리거 성공: ID={Id}", id);
                    TempData["SuccessMessage"] = "알람이 성공적으로 트리거되었습니다.";
                }
                else
                {
                    _logger.LogWarning("알람 트리거 실패: ID={Id}", id);
                    TempData["ErrorMessage"] = "알람 트리거에 실패했습니다. 다시 시도해 주세요.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "알람 트리거 중 오류 발생: ID={Id}, 오류={ErrorMessage}", id, ex.Message);

                TempData["ErrorMessage"] = $"알람 트리거 중 오류가 발생했습니다: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}

