using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virtual_Machine
{
    public class VirtualMachine
    {
        CPU m_cpu;
        Bios m_bios;
        Display m_display;
        PlatformControlHub m_PCH;
        VMKeyboard m_keyboard;
        RAM m_RAM;
        BlockDevice m_SSD;

        InterconnectTerminal m_CPU_PCH_Interconnect = new InterconnectTerminal(1, 10);
        InterconnectTerminal m_PCH_CPU_Interconnect = new InterconnectTerminal(1, 10);

        InterconnectTerminal m_PCH_BIOS_Interconnect = new InterconnectTerminal(32, 10);
        InterconnectTerminal m_BIOS_PCH_Interconnect = new InterconnectTerminal(32, 10);

        InterconnectTerminal m_PCH_Display_Interconnect = new InterconnectTerminal(32, 10);
        InterconnectTerminal m_Display_PCH_Interconnect = new InterconnectTerminal(32, 10);

        InterconnectTerminal m_PCH_Keyboard_Interconnect = new InterconnectTerminal(32, 10);
        InterconnectTerminal m_Keyboard_PCH_Interconnect = new InterconnectTerminal(32, 10);

        InterconnectTerminal m_CPU_RAM_Interconnect = new InterconnectTerminal(1, 10);
        InterconnectTerminal m_RAM_CPU_Interconnect = new InterconnectTerminal(1, 10);

        InterconnectTerminal m_PCH_SSD_Interconnect = new InterconnectTerminal(32, 10);
        InterconnectTerminal m_SSD_PCH_Interconnect = new InterconnectTerminal(32, 10);

        public const uint PICAddress = 32;
        public const uint PCHStartAddress = 512;
        public const uint biosStartAddress = PCHStartAddress; // 512
        public const uint displayStartAddress = biosStartAddress + 1024; // 1536
        public const uint displayCommandAddress = displayStartAddress + 2048; // 3584
        public const uint displayFgColourAddress = displayCommandAddress + 1; // 3585
        public const uint displayBkgColourAddress = displayCommandAddress + 2; // 3586
        public const uint keyboardStartAddress = displayCommandAddress + 4; // 3588

        public const uint RAMStartAddress = keyboardStartAddress + 1; // Keep this at the top of the memory space for organisational convenience. // 3589
        public const uint RAMSize = 128 * 1024 * 1024; // 128MB

        public const uint SSDSeekAddress = RAMStartAddress + RAMSize; // 134221317
        public const uint SSDFIFOAddress = SSDSeekAddress + 1; // 134221318
        public const uint SSDInterruptAcknowledgeAddress = SSDFIFOAddress + 1; // 134221319

        List<InterconnectTerminal> m_interconnects = new List<InterconnectTerminal>();

        uint tickCount;

        public static StatsCounters Counters;

        public CPU Processor { get { return m_cpu; } }
        public Bios BIOS { get { return m_bios; } }
        public RAM RAM { get { return m_RAM; } }


        public VirtualMachine()
        {
            m_interconnects.Add(m_CPU_PCH_Interconnect);
            m_interconnects.Add(m_PCH_CPU_Interconnect);
            m_interconnects.Add(m_PCH_BIOS_Interconnect);
            m_interconnects.Add(m_BIOS_PCH_Interconnect);
            m_interconnects.Add(m_PCH_Display_Interconnect);
            m_interconnects.Add(m_Display_PCH_Interconnect);
            m_interconnects.Add(m_PCH_Keyboard_Interconnect);
            m_interconnects.Add(m_Keyboard_PCH_Interconnect);
            m_interconnects.Add(m_RAM_CPU_Interconnect);
            m_interconnects.Add(m_CPU_RAM_Interconnect);
            m_interconnects.Add(m_SSD_PCH_Interconnect);
            m_interconnects.Add(m_PCH_SSD_Interconnect);

            m_CPU_PCH_Interconnect.SetOtherEnd(m_PCH_CPU_Interconnect);
            m_PCH_BIOS_Interconnect.SetOtherEnd(m_BIOS_PCH_Interconnect);
            m_PCH_Display_Interconnect.SetOtherEnd(m_Display_PCH_Interconnect);
            m_PCH_Keyboard_Interconnect.SetOtherEnd(m_Keyboard_PCH_Interconnect);
            m_RAM_CPU_Interconnect.SetOtherEnd(m_CPU_RAM_Interconnect);
            m_PCH_SSD_Interconnect.SetOtherEnd(m_SSD_PCH_Interconnect);

            m_cpu = new CPU(m_CPU_PCH_Interconnect, m_CPU_RAM_Interconnect);

            m_RAM = new RAM(m_RAM_CPU_Interconnect, (int)RAMSize, RAMStartAddress);

            m_bios = new Bios(biosStartAddress, m_BIOS_PCH_Interconnect);
            m_display = new Display(displayStartAddress, m_Display_PCH_Interconnect);
            m_keyboard = new VMKeyboard(m_Keyboard_PCH_Interconnect);
            m_SSD = new BlockDevice((int)SSDSeekAddress, m_SSD_PCH_Interconnect, @"MainDrive", 1024 * 1024, 200000, 1); // 4GB, 0.1ms seek time

            m_PCH = new PlatformControlHub(m_PCH_CPU_Interconnect, PCHStartAddress);

            m_PCH.AddDevice(m_PCH_BIOS_Interconnect, biosStartAddress);
            m_PCH.AddDevice(m_PCH_Display_Interconnect, displayStartAddress);
            m_PCH.AddDevice(m_PCH_Keyboard_Interconnect, keyboardStartAddress);
            m_PCH.AddDevice(m_PCH_SSD_Interconnect, SSDSeekAddress);
        }

        public void Run()
        {
            while (true)
            {
                Tick();
            }
        }

        public void Tick()
        {
            tickCount++;

            m_cpu.Tick();

            m_RAM.Tick();

            m_PCH.Tick();

            m_bios.Tick();
            m_display.Tick();
            m_SSD.Tick();

            if (tickCount % 100000 == 0)
            {
                m_keyboard.Tick();
            }

            foreach (InterconnectTerminal interconnect in m_interconnects)
            {
                interconnect.Tick();
            }
        }
    }
}
