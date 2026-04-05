using Microsoft.EntityFrameworkCore;

namespace JO_UNI_Guide.Helpers
{
    public class PaginatedList<T> : List<T>
    {
        public int PageIndex {get; private set;} //رقم الصفحة الحالية
        public int TotalPages {get; private set;} // اجمالي عدد الصفحات جميعها

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize) 
        {
            PageIndex = pageIndex;            // لمعرفة كم صفحة بنحتاح لعرض الجامعات يعني لو عندي 12 الجامعة بقسمهم على 5 بنفس الصحفة بطلع بحتاج 3 صفحات 
            TotalPages = (int)Math.Ceiling(count/(double)pageSize); 
            this.AddRange(items);
        }
        // هل في صفحة سابقة ؟
        public bool HasPreviousPage => PageIndex > 1; 
        // هل فس صفحة تالية ؟
        public bool HasNextPage => PageIndex < TotalPages;

        //لجلب الداتا المقسمة من الداتا بيز
        public static async Task<PaginatedList<T>> CreateAsync (IQueryable<T> source , int pageIndex , int pageSize) 
        {
            //لتحديد كل العناصر بالبداية 
            var count = await source.CountAsync();

            //في عملية التقسيم رح نتحتاج للSkip , Take
            // بتفشق عن الصحفات السابقة وبتاخذ العدد المطلوب للصحفة الحالية
            var items = await source.Skip((pageIndex - 1)*pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T> (items, count, pageIndex, pageSize);
        }
    }
}
