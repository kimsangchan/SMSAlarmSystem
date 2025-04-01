// 작성자: Sangchan, Kim
// 작성일: 2025-04-01
// 기능: 페이지네이션 관련 기능을 제공하는 헬퍼 클래스
// 설명: 컨트롤러에서 공통으로 사용하는 페이지네이션 로직을 구현합니다.

using Microsoft.Extensions.Logging;
using SMSAlarmSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SMSAlarmSystem.Helpers
{
    /// <summary>
    /// 페이지네이션 관련 기능을 제공하는 헬퍼 클래스
    /// </summary>
    /// <typeparam name="T">페이지네이션할 데이터 타입</typeparam>
    public class PaginationHelper<T> where T : class
    {
        private readonly ILogger<PaginationHelper<T>> _logger;

        /// <summary>
        /// PaginationHelper 생성자
        /// </summary>
        /// <param name="logger">로깅을 위한 ILogger 인스턴스</param>
        /// <exception cref="ArgumentNullException">logger가 null인 경우 발생</exception>
        public PaginationHelper(ILogger<PaginationHelper<T>> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 페이지네이션을 적용한 뷰 모델을 생성합니다.
        /// </summary>
        /// <param name="items">전체 아이템 목록</param>
        /// <param name="page">현재 페이지 번호 (1부터 시작)</param>
        /// <param name="pageSize">페이지당 아이템 수</param>
        /// <returns>페이지네이션이 적용된 뷰 모델</returns>
        public PaginatedViewModel<T> CreatePaginatedModel(
            IEnumerable<T> items,
            int page,
            int pageSize)
        {
            try
            {
                // null 체크
                if (items == null)
                {
                    _logger.LogWarning("페이지네이션 적용 실패: 항목 목록이 null입니다.");
                    return new PaginatedViewModel<T>();
                }

                // 페이지 번호와 페이지 크기 유효성 검사
                page = Math.Max(1, page);
                pageSize = Math.Clamp(pageSize, 1, 100);

                var totalItems = items.Count();
                var pagedItems = items.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                // 계산 속성을 사용하므로 TotalPages, HasPreviousPage, HasNextPage는 설정하지 않음
                return new PaginatedViewModel<T>
                {
                    Items = pagedItems,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalItems
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "페이지네이션 적용 중 오류 발생: {ErrorMessage}", ex.Message);
                return new PaginatedViewModel<T>();
            }
        }
    }
}
