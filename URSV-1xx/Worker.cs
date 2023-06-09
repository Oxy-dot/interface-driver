using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskCore;
using URSV1xx.Data;
using XLiFrameWork;

namespace URSV1xx
{
    public partial class USRV_1xxMeterDriver
    {
        private static Result CreateFailureResult(int errorCode, int archType  )
        {
            return new Result {
                Status = CurrentTaskStatus.Failed,
                ErrorCode = Math.Max( 5000, errorCode),
                StoredProcedureParameters = new List<BaseWorker.SpParam> {
                    new BaseWorker.SpParam { ParamName = "@SrvErrorCode", DbType = SqlDbType.Int, IsOutput = false, Value = Math.Max(50000, errorCode) },
                    new BaseWorker.SpParam { ParamName = "@CurrentTimeDevice", DbType = SqlDbType.DateTime, IsOutput = false, Value = DBNull.Value },
                    new BaseWorker.SpParam { ParamName = "@TimeRead", DbType = SqlDbType.DateTime, IsOutput = false, Value = DateTime.UtcNow },
                    new BaseWorker.SpParam { ParamName = "@DataTV", DbType = SqlDbType.Structured, IsOutput = false, Value = new List<TV>().ToDataTable("DataTV") },
                    new BaseWorker.SpParam { ParamName = "@DataTR", DbType = SqlDbType.Structured, IsOutput = false, Value = new List<TR>().ToDataTable("DataTR") },
                    new BaseWorker.SpParam { ParamName = "@DataNS", DbType = SqlDbType.Structured, IsOutput = false, Value = new List<NS>().ToDataTable("DataNS") },
                    new BaseWorker.SpParam { ParamName = "@DataInternalUOM", DbType = SqlDbType.Structured, IsOutput = false, Value = new List<DataInternalUOM>().ToDataTable("DataInternalUOM") },
                    new BaseWorker.SpParam { ParamName = "@TypeArch", DbType = SqlDbType.NVarChar, IsOutput = false, Value = archType },
                    new BaseWorker.SpParam { ParamName = "@FullArchive", DbType = SqlDbType.Bit, IsOutput = false, Value = false },
                }
            };
        }
        private static Result CreateSuccessfulResult(IEnumerable<TV> aDataTV, IEnumerable<TR> aDataTR, IEnumerable<NS> aDataNS,IEnumerable<DataInternalUOM> aDataUOM, int? archType)
        {
            return new Result
            {
                Status = CurrentTaskStatus.Success,
                ErrorCode = 0,
                StoredProcedureParameters = new List<BaseWorker.SpParam>
                {
                    new BaseWorker.SpParam { ParamName = "@SrvErrorCode", DbType = SqlDbType.Int, IsOutput = false, Value = 0 },
                    new BaseWorker.SpParam { ParamName = "@DataTV", DbType = SqlDbType.Structured, IsOutput = false, Value = aDataTV.ToDataTable("DataTV") },
                    new BaseWorker.SpParam { ParamName = "@DataNS", DbType = SqlDbType.Structured, IsOutput = false, Value = aDataNS.ToDataTable("DataNS") },
                    new BaseWorker.SpParam { ParamName = "@DataTR", DbType = SqlDbType.Structured, IsOutput = false, Value = aDataTR.ToDataTable("DataTR") },
                    new BaseWorker.SpParam { ParamName = "@DataInternalUOM", DbType = SqlDbType.Structured, IsOutput = false, Value = aDataUOM.ToDataTable("DataInternalUOM") },
                    new BaseWorker.SpParam { ParamName = "@TimeRead", DbType = SqlDbType.DateTime, IsOutput = false, Value = DateTime.UtcNow },
                    new BaseWorker.SpParam { ParamName = "@TypeArch", DbType = SqlDbType.NVarChar, IsOutput = false, Value = archType ?? 0 },
                    new BaseWorker.SpParam { ParamName = "@FullArchive", DbType = SqlDbType.Bit, IsOutput = false, Value = true },
                    new BaseWorker.SpParam { ParamName = "@CurrentTimeDevice",DbType = SqlDbType.DateTime, IsOutput = false, Value = DBNull.Value },
                }
            };
        }
    }
}
