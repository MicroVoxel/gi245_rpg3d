using UnityEngine;
using System;

/// <summary>
/// MyActions: ศูนย์กระจายคำสั่งและการกระทำ (Event / Action Broker) 
/// ใช้เชื่อมต่อระบบ Logic ในตัวละคร เข้ากับระบบสร้าง VFX และระบบแปลงข้อมูลเวทมนตร์
/// </summary>
public static class MyActions
{
    /// <summary>
    /// Event เมื่อเริ่มโหลด/ชาร์จเวทมนตร์ (เช่น เสกวงแหวนเวทย์ใต้เท้าฮีโร่)
    /// <para>พารามิเตอร์: int (Load ID VFX), Vector3 (ตำแหน่งชาร์จเวทย์), float (ระยะเวลาโหลดพลัง)</para>
    /// </summary>
    public static Action<int, Vector3, float> onLoadMagic;

    /// <summary>
    /// Event เมื่อเวทมนตร์เริ่มยิง/แสดงผล (เช่น เสกขีปนาวุธพุ่งหาศัตรู หรือร่ายบัฟลงตัวผู้ใช้)
    /// <para>พารามิเตอร์: int (Shoot ID VFX), Vector3 (ตำแหน่งเกิดของเอฟเฟกต์), Vector3 (ตำแหน่งเป้าหมาย), float (เวลาเดินทางของเอฟเฟกต์)</para>
    /// </summary>
    public static Action<int, Vector3, Vector3, float> onShootMagic;

    /// <summary>
    /// Function สำหรับเข้าถึงฐานข้อมูลเพื่อสร้างวัตถุ Magic ชิ้นใหม่ขึ้นมาใช้งานจริง (Runtime Object) จากตัวเลข ID
    /// <para>พารามิเตอร์ส่งเข้า: int (ID ของเวทย์)</para>
    /// <para>ค่าส่งกลับ: Magic (ออบเจกต์เวทมนตร์ที่พร้อมใช้สวมใส่ให้ตัวละคร)</para>
    /// </summary>
    public static Func<int, Magic> onCreateMagic;
}