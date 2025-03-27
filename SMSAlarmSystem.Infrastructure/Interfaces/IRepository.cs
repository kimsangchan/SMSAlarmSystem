// 작성자: Sangchan, Kim
// 작성일: 2025-03-27
// 기능: 기본 저장소 인터페이스
// 설명: 이 인터페이스는 모든 저장소 클래스가 구현해야 하는 기본 메서드를 정의합니다.
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SMSAlarmSystem.Core.Interfaces
{
    public interface IRepository<T> where T : class
    {
        // 모든 항목 가져오기
        Task<IEnumerable<T>> GetAllAsync();

        // 조건에 맞는 항목 가져오기
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        // ID로 항목 가져오기
        Task<T?> GetByIdAsync(int id);

        // 항목 추가
        Task AddAsync(T entity);

        // 항목 업데이트
        Task UpdateAsync(T entity);

        // 항목 삭제
        Task DeleteAsync(T entity);
    }
}
