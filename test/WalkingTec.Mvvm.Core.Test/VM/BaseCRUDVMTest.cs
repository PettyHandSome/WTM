﻿using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WalkingTec.Mvvm.Core.Test.VM
{
    [TestClass]
    public class BaseCRUDVMTest
    {
        private BaseCRUDVM<School> _schoolvm = new BaseCRUDVM<School>();
        private BaseCRUDVM<Major> _majorvm = new BaseCRUDVM<Major>();
        private BaseCRUDVM<Student> _studentvm = new BaseCRUDVM<Student>();
        private string _seed;
        private Dictionary<string, string> _msd;

        public BaseCRUDVMTest()
        {
            _seed = Guid.NewGuid().ToString();
            _schoolvm.DC = new DataContext(_seed, DBTypeEnum.Memory);
            _majorvm.DC = new DataContext(_seed, DBTypeEnum.Memory);
            _studentvm.DC = new DataContext(_seed, DBTypeEnum.Memory);

            _schoolvm.Session = new MockSession();
            _majorvm.Session = new MockSession();
            _studentvm.Session = new MockSession();

            _schoolvm.MSD = new MockMSD();
            _majorvm.MSD = new MockMSD();
            _studentvm.MSD = new MockMSD();
            
            _schoolvm.Session.Set("UserInfo", new LoginUserInfo { ITCode = "schooluser" });
            _majorvm.Session.Set("UserInfo", new LoginUserInfo { ITCode = "majoruser" });
            _studentvm.Session.Set("UserInfo", new LoginUserInfo { ITCode = "studentuser" });
        }

        [TestMethod]
        [Description("单表添加")]
        [DataTestMethod]
        [DataRow("111", "test1", SchoolTypeEnum.PRI, "remark1")]
        [DataRow("222","test2", SchoolTypeEnum.PUB,"remark2")]
        public void SingleTableDoAdd(string code, string name, SchoolTypeEnum schooltype, string remark)
        {
            School s = new School();
            s.SchoolCode = code;
            s.SchoolName = name;
            s.SchoolType = schooltype;
            s.Remark = remark;
            _schoolvm.Entity = s;
            _schoolvm.DoAdd();

            using (var context = new DataContext(_seed, DBTypeEnum.Memory))
            {
                var rv = context.Set<School>().ToList();
                Assert.AreEqual(1, rv.Count());
                Assert.AreEqual(code, rv[0].SchoolCode);
                Assert.AreEqual(name, rv[0].SchoolName);
                Assert.AreEqual(schooltype, rv[0].SchoolType);
                Assert.AreEqual(remark, rv[0].Remark);
                Assert.AreEqual("schooluser", rv[0].CreateBy);
                Assert.IsTrue(DateTime.Now.Subtract(rv[0].CreateTime.Value).Seconds < 10);
            }
            Assert.IsTrue(_schoolvm.MSD.Count == 0);
        }

        [TestMethod]
        [Description("单表修改全部字段")]
        [DataTestMethod]
        [DataRow("111", "test1", SchoolTypeEnum.PRI, "remark1")]
        [DataRow("222", "test2", SchoolTypeEnum.PUB, "remark2")]
        public void SingleTableDoEdit(string code, string name, SchoolTypeEnum schooltype, string remark)
        {
            School s = new School();
            s.SchoolCode = "000";
            s.SchoolName = "default";
            s.SchoolType = null;
            s.Remark = "default";
            _schoolvm.Entity = s;
            _schoolvm.DoAdd();

            using (var context = new DataContext(_seed, DBTypeEnum.Memory))
            {
                School s2 = new School();
                s2.SchoolCode = code;
                s2.SchoolName = name;
                s2.SchoolType = schooltype;
                s2.Remark = remark;
                s2.ID = s.ID;
                _schoolvm.DC = context;
                _schoolvm.Entity = s2;
                _schoolvm.DoEdit(true);
            }

            using (var context = new DataContext(_seed, DBTypeEnum.Memory))
            {
                var rv = context.Set<School>().ToList();
                Assert.AreEqual(1, rv.Count());
                Assert.AreEqual(code, rv[0].SchoolCode);
                Assert.AreEqual(name, rv[0].SchoolName);
                Assert.AreEqual(schooltype, rv[0].SchoolType);
                Assert.AreEqual(remark, rv[0].Remark);
                Assert.AreEqual("schooluser", rv[0].UpdateBy);
                Assert.IsTrue(DateTime.Now.Subtract(rv[0].UpdateTime.Value).Seconds < 10);
            }
        }


        [TestMethod]
        [Description("单表修改指定字段")]
        [DataTestMethod]
        [DataRow("111", "test1", SchoolTypeEnum.PRI, "remark1")]
        [DataRow("222", "test2", SchoolTypeEnum.PUB, "remark2")]
        public void SingleTableDoEditFields(string code, string name, SchoolTypeEnum schooltype, string remark)
        {
            School s = new School();
            s.SchoolCode = "000";
            s.SchoolName = "default";
            s.SchoolType = null;
            s.Remark = "default";
            _schoolvm.Entity = s;
            _schoolvm.DoAdd();

            using (var context = new DataContext(_seed, DBTypeEnum.Memory))
            {
                School s2 = new School();
                s2.SchoolCode = code;
                s2.SchoolName = name;
                s2.SchoolType = schooltype;
                s2.Remark = remark;
                s2.ID = s.ID;
                _schoolvm.DC = context;
                _schoolvm.Entity = s2;
                _schoolvm.FC.Add("Entity.SchoolName", name);
                _schoolvm.DoEdit();
            }

            using (var context = new DataContext(_seed, DBTypeEnum.Memory))
            {
                var rv = context.Set<School>().ToList();
                Assert.AreEqual(1, rv.Count());
                Assert.AreEqual("000", rv[0].SchoolCode);
                Assert.AreEqual(name, rv[0].SchoolName);
                Assert.AreEqual(null, rv[0].SchoolType);
                Assert.AreEqual("default", rv[0].Remark);
                Assert.AreEqual("schooluser", rv[0].UpdateBy);
                Assert.IsTrue(DateTime.Now.Subtract(rv[0].UpdateTime.Value).Seconds < 10);
            }
        }


        [TestMethod]
        [Description("单表删除")]
        public void SingleTableDelete()
        {
            School s = new School();
            s.SchoolCode = "000";
            s.SchoolName = "default";
            s.SchoolType = null;
            s.Remark = "default";
            _schoolvm.Entity = s;
            _schoolvm.DoAdd();

            using (var context = new DataContext(_seed, DBTypeEnum.Memory))
            {
                Assert.AreEqual(1, context.Set<School>().Count());
                _schoolvm.DC = context;
                _schoolvm.Entity = new School { ID = s.ID };
                _schoolvm.DoDelete();
            }

            using (var context = new DataContext(_seed, DBTypeEnum.Memory))
            {
                Assert.AreEqual(0, context.Set<School>().Count());
            }
        }

        [TestMethod]
        [Description("Persist单表删除")]
        public void SingleTablePersistDelete()
        {
            Student s = new Student();
            s.LoginName = "a";
            s.Password = "b";
            s.Name = "ab";
            _studentvm.Entity = s;
            _studentvm.DoAdd();

            using (var context = new DataContext(_seed, DBTypeEnum.Memory))
            {
                Assert.AreEqual(1, context.Set<Student>().Count());
                _studentvm.DC = context;
                _studentvm.Entity = new Student { ID = s.ID };
                _studentvm.DoDelete();
            }

            using (var context = new DataContext(_seed, DBTypeEnum.Memory))
            {
                Assert.AreEqual(1, context.Set<Student>().Count());
                var rv = context.Set<Student>().ToList()[0];
                Assert.AreEqual("studentuser", rv.UpdateBy);
                Assert.IsTrue(DateTime.Now.Subtract(rv.UpdateTime.Value).Seconds < 10);
            }

        }

        [TestMethod]
        [Description("一对多主表删除")]
        public void One2ManyTableDelete()
        {
            One2ManyDoAdd();

            using (var context = new DataContext(_seed, DBTypeEnum.Memory))
            {
                var id = context.Set<School>().AsNoTracking().First().ID;
                _schoolvm.DC = context;
                _schoolvm.Entity = new School { ID = id };
                _schoolvm.DoDelete();
            }

            using (var context = new DataContext(_seed, DBTypeEnum.Memory))
            {
                Assert.AreEqual(0, context.Set<School>().Count());
            }
        }

        [TestMethod]
        [Description("多对多主表删除")]
        public void Many2ManyTableDelete()
        {
            Many2ManyDoAdd();

            using (var context = new DataContext(_seed, DBTypeEnum.Memory))
            {
                var id = context.Set<Student>().AsNoTracking().First().ID;
                _studentvm.DC = context;
                _studentvm.Entity = new Student { ID = id };
                _studentvm.DoRealDelete();
            }

            using (var context = new DataContext(_seed, DBTypeEnum.Memory))
            {
                Assert.AreEqual(1, context.Set<Student>().Count(), "主表是否正确删除");
                //Assert.AreEqual(1, context.Set<StudentMajor>().Count(), "子表是否正确删除");
            }
        }

        [TestMethod]
        [Description("Persist单表物理删除")]
        public void SingleTablePersistRealDelete()
        {
            Student s = new Student();
            s.LoginName = "a";
            s.Password = "b";
            s.Name = "ab";
            _studentvm.Entity = s;
            _studentvm.DoAdd();

            using (var context = new DataContext(_seed, DBTypeEnum.Memory))
            {
                Assert.AreEqual(1, context.Set<Student>().Count());
                _studentvm.DC = context;
                _studentvm.Entity = new Student { ID = s.ID };
                _studentvm.DoRealDelete();
            }

            using (var context = new DataContext(_seed, DBTypeEnum.Memory))
            {
                Assert.AreEqual(0, context.Set<Student>().Count());
            }

        }

        [TestMethod]
        [Description("一对多添加")]
        public void One2ManyDoAdd()
        {
            School s = new School();
            s.SchoolCode = "000";
            s.SchoolName = "school";
            s.SchoolType = SchoolTypeEnum.PRI;
            s.Majors = new List<Major>();
            s.Majors.Add(new Major
            {
                MajorCode = "111",
                MajorName = "major1",
                MajorType = MajorTypeEnum.Optional
            });
            s.Majors.Add(new Major
            {
                MajorCode = "222",
                MajorName = "major2",
                MajorType = MajorTypeEnum.Required
            });
            _schoolvm.Entity = s;
            _schoolvm.DoAdd();

            using (var context = new DataContext(_seed, DBTypeEnum.Memory))
            {
                Assert.AreEqual(1, context.Set<School>().Count());
                Assert.AreEqual(2, context.Set<Major>().Count());
                var rv = context.Set<Major>().ToList();
                Assert.AreEqual("111", rv[0].MajorCode);
                Assert.AreEqual("major1", rv[0].MajorName);
                Assert.AreEqual(MajorTypeEnum.Optional, rv[0].MajorType);
                Assert.AreEqual("222", rv[1].MajorCode);
                Assert.AreEqual("major2", rv[1].MajorName);
                Assert.AreEqual(MajorTypeEnum.Required, rv[1].MajorType);

                Assert.AreEqual("schooluser", context.Set<School>().First().CreateBy);
                Assert.IsTrue(DateTime.Now.Subtract(context.Set<School>().First().CreateTime.Value).Seconds < 10);
                Assert.AreEqual("schooluser", rv[0].CreateBy);
                Assert.IsTrue(DateTime.Now.Subtract(rv[0].CreateTime.Value).Seconds < 10);
                Assert.AreEqual("schooluser", rv[1].CreateBy);
                Assert.IsTrue(DateTime.Now.Subtract(rv[1].CreateTime.Value).Seconds < 10);

            }
        }

        [TestMethod]
        [Description("多对多添加")]
        public void Many2ManyDoAdd()
        {
            Major m1 = new Major
            {
                MajorCode = "111",
                MajorName = "major1",
                MajorType = MajorTypeEnum.Optional
            };
            Major m2 = new Major
            {
                MajorCode = "222",
                MajorName = "major2",
                MajorType = MajorTypeEnum.Required
            };
            Student s1 = new Student
            {
                LoginName = "s1",
                Password = "aaa",
                Name = "student1"
            };
            Student s2 = new Student
            {
                LoginName = "s2",
                Password = "bbb",
                Name = "student2"
            };
            _majorvm.Entity = m1;
            _majorvm.DoAdd();
            _majorvm.Entity = m2;
            _majorvm.DoAdd();

            s1.StudentMajor = new List<StudentMajor>();
            s1.StudentMajor.Add(new StudentMajor { MajorId = m1.ID });
            s2.StudentMajor = new List<StudentMajor>();
            s2.StudentMajor.Add(new StudentMajor { MajorId = m2.ID });
            _studentvm.Entity = s1;
            _studentvm.DoAdd();
            _studentvm.Entity = s2;
            _studentvm.DoAdd();

            using (var context = new DataContext(_seed, DBTypeEnum.Memory))
            {
                Assert.AreEqual(2, context.Set<Major>().Count());
                Assert.AreEqual(2, context.Set<Student>().Count());
                Assert.AreEqual(2, context.Set<StudentMajor>().Count());
                var rv = context.Set<StudentMajor>().ToList();
                Assert.AreEqual(s1.ID, rv[0].StudentId);
                Assert.AreEqual(m1.ID, rv[0].MajorId);
                Assert.AreEqual(s2.ID, rv[1].StudentId);
                Assert.AreEqual(m2.ID, rv[1].MajorId);
            }
        }

    }
}
