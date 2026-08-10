/****************************************************************************
 * IDataController — 数据控制器接口
 * 
 * 职责：定义数据控制器的通用接口
 ****************************************************************************/

namespace HotUpdate.LocalStorageKit
{
    public interface IDataController
    {
        SaveMode SaveMode { get; }
        bool IsDirty { get; }
        void Save();
        void Load();
        void ClearDirty();
    }
}
